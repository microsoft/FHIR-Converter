// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var version = '1.0';

var messageEditor;
var templateCodeEditor;
var outputCode;
var lineMapping = []; // Mapping source (template) to destination (output) lines
var lastScrollPositions = {};
var skipScrollHandlers = {};
var lastScrollEditors = [];
var activeTemplate = { name: 'untitled', parent: null, data: '{}', active: true, marks: [] };
var openTemplates = [activeTemplate];
var currentTemplateReference = { [activeTemplate.name]: activeTemplate.data }; // This is what we set the template to initially
var latestRequest = 0;
var nextRequestCall;
var waitForTypingTimeout = 100;
var bannerFade;
var transitionTime = 1000;
var uncheckedApiKey = false;
// eslint-disable-next-line no-unused-vars
var templateNames;

// The chosen color schemes also need to have their stylesheets linked in the index.html file
var darkMode = "blackboard";
var lightMode = "eclipse";

var currentEditorSettings = { scrollSync: true, darkMode: false };
var apiKey = '';
var maskedApiKey = '***********';
var hasSuccessfulCallBeenMade = false;

function setLastScrollPosition(editor) {
    if (lastScrollEditors.indexOf(editor) < 0) lastScrollEditors.push(editor);
    lastScrollPositions['editor.' + lastScrollEditors.indexOf(editor)] = editor.getScrollInfo();
}

function getLastScrollPosition(editor) {
    return lastScrollPositions['editor.' + lastScrollEditors.indexOf(editor)];
}

function setSkipNextScrollHandler(editor, state) {
    if (lastScrollEditors.indexOf(editor) < 0) lastScrollEditors.push(editor);
    skipScrollHandlers['editor.' + lastScrollEditors.indexOf(editor)] = state;
}

function shouldSkipNextScrollHandler(editor) {
    if (lastScrollEditors.indexOf(editor) < 0) lastScrollEditors.push(editor);
    return skipScrollHandlers['editor.' + lastScrollEditors.indexOf(editor)];
}

function adjustScrolling(scrollSourceEditor, scrollTargetEditor, sourceLines, targetLines) {
    if (!getSettings().scrollSync) {
        return;
    }

    if (shouldSkipNextScrollHandler(scrollSourceEditor)) {
        setSkipNextScrollHandler(scrollSourceEditor, false);
        return;
    }

    var sourceScrollInfo = scrollSourceEditor.getScrollInfo();
    var targetScrollInfo = scrollTargetEditor.getScrollInfo();

    var sourceLineTop = scrollSourceEditor.coordsChar({ left: sourceScrollInfo.left, top: sourceScrollInfo.top }, "local").line;
    var sourceLineBottom = scrollSourceEditor.coordsChar({ left: sourceScrollInfo.left, top: sourceScrollInfo.top + sourceScrollInfo.clientHeight }, "local").line;
    var sourceRange = { top: sourceLineTop, bottom: sourceLineBottom };

    var targetLineTop = scrollTargetEditor.coordsChar({ left: targetScrollInfo.left, top: targetScrollInfo.top }, "local").line;
    var targetLineBottom = scrollTargetEditor.coordsChar({ left: targetScrollInfo.left, top: targetScrollInfo.top + targetScrollInfo.clientHeight }, "local").line;
    var currentTargetRange = { top: targetLineTop, bottom: targetLineBottom };

    // Map based on current line mapping
    var destinationRange = sourceRangeToDestinationRange(sourceRange, sourceLines, targetLines);

    // If the destination range is taller than the editor size, 
    // we need to adjust the destination range:
    //   if (scrolling down) trim the top
    //   else trim the bottom of the range

    var destinationHeightLines = destinationRange.bottom - destinationRange.top;
    var targetHeightLines = currentTargetRange.bottom - currentTargetRange.top;

    var lastScrollInfo = getLastScrollPosition(scrollSourceEditor) || sourceScrollInfo;
    setLastScrollPosition(scrollSourceEditor);

    if (lastScrollInfo.top == sourceScrollInfo.top) {
        return;
    } else if (lastScrollInfo.top > sourceScrollInfo.top) {
        // Scrolling up
        if (destinationHeightLines > targetHeightLines) {
            destinationRange.bottom = destinationRange.bottom - (destinationHeightLines - targetHeightLines);
        }
    } else {
        if (destinationHeightLines > targetHeightLines) {
            destinationRange.top = destinationRange.top + (destinationHeightLines - targetHeightLines);
        }
    }

    setSkipNextScrollHandler(scrollTargetEditor, true); // So we don't trigger while we manipulate scroll
    scrollTargetEditor.scrollIntoView({ from: { line: destinationRange.top, ch: 0 }, to: { line: destinationRange.bottom, ch: 0 } });
}

function getSettings() {
    return currentEditorSettings;
}

function setSettings(settings) {
    currentEditorSettings = settings;
}

function getApiKey() {
    return ((apiKey === maskedApiKey) ? '' : apiKey);
}

function checkApiKey(successFunc, errorFunc) {
    $.ajax('/api/templates', {
        'type': 'GET',
        'processData': false,
        'beforeSend': function (request) {
            request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
        },
        'success': function (templateList) {
            hasSuccessfulCallBeenMade = true;
            templateList.templates.map(template => template.templateName);
            // eslint-disable-next-line no-undef
            initHelperList();
            uncheckedApiKey = false;
            if (successFunc) successFunc();
        },
        'error': function () {
            hasSuccessfulCallBeenMade = false;
            if (errorFunc) errorFunc();
        }
    });
}

function checkApiKeyAndRaiseModal(userRequestedValidation) {
    checkApiKey(
        function () {
            $('#settings-modal').modal('hide');
            $('#settings-modal-alert').hide();
        },
        function () {
            $('#settings-modal').modal('show');
            if ($('#api-key-input').val() || userRequestedValidation) {
                $('#settings-modal-alert').show();
                $('#settings-modal-alert').html("<strong>Error: </strong>Invalid API Key");
            }
        }
    );
}

/* 
    Give a sourceRange ({top: lineNum, bottom: lineNum})
    and two arrays of source and destination lines
    map the sourceRange to a destinationRange.
*/
function sourceRangeToDestinationRange(sourceRange, sourceLines, destinationLines) {
    if (sourceLines.length != destinationLines.length) {
        throw Error('Source and destination arrays lengths do not match');
    }

    // Assume destination range is the max range and we will narrow it below
    var topTarget = { source: 0, destination: Math.min(...destinationLines) }; // mapping.reduce((prev, current) => (prev.destination < current.destination) ? prev : current);
    var bottomTarget = { source: Math.max(...sourceLines), destination: Math.max(...destinationLines) }; // mapping.reduce((prev, current) => (prev.destination > current.destination) ? prev : current);

    for (var i = 0; i < sourceLines.length; i++) {
        // Is this a good candidate for the top target
        if (
            (sourceLines[i] <= sourceRange.top) && // If this one is above the current source range
            (sourceRange.top - sourceLines[i]) <= (sourceRange.top - topTarget.source) && // If it is closer to the line we currently have
            (destinationLines[i] > topTarget.destination) // If the destination is further up
        ) {
            topTarget = { source: sourceLines[i], destination: destinationLines[i] };
        }

        // Is this a good candidate for the bottom target
        if (
            (sourceLines[i] >= sourceRange.bottom) && // If this line is below the sour target
            (sourceLines[i] - sourceRange.bottom) <= (bottomTarget.source - sourceRange.bottom) && // If it is closer to the line we currently have
            (destinationLines[i] < bottomTarget.destination) // If the destination is further down
        ) {
            bottomTarget = { source: sourceLines[i], destination: destinationLines[i] };
        }
    }

    return { top: topTarget.destination, bottom: bottomTarget.destination };
}

function convertMessage(resetOutputScroll) {
    if (nextRequestCall) {
        clearTimeout(nextRequestCall);
    }

    nextRequestCall = setTimeout(() => {
        var reqBody = {};
        var replacementDictionary = {};
        var templateLines = [];
        var outputLines = [];

        var scrollInfo = outputCode.getScrollInfo();

        if (messageEditor.getValue()) {
            reqBody.messageBase64 = btoa(messageEditor.getValue());
        }

        var topTemplate = openTemplates.find(template => template.parent === null);
        if (topTemplate) {
            templateLines = topTemplate.data.replace(/(?:\r\n|\r|\n)/g, '\n').split('\n');

            if (activeTemplate === topTemplate && getSettings().scrollSync) {
                // Match the first property, e.g., '"propname":'
                // Note, we will exclude properties that have a $ in the name, 
                // since we will use that as a label. 
                var propertyRegEx = /"[^("|$|{|})]+":/;
                var excludeRegex = /entry|resource|resourceType|id|meta|versionId/; // for using these keys on server side
                for (var i = 0; i < templateLines.length; i++) {
                    var lineProps = templateLines[i].match(propertyRegEx);
                    if (lineProps && lineProps.length > 0 && !excludeRegex.test(templateLines[i])) {
                        var placeHolder = '"$' + i + '$":';
                        replacementDictionary[placeHolder] = lineProps[0];

                        // Replacing on the regex instead of the actual match, 
                        // since we could have multiple matches and we just want the first.
                        templateLines[i] = templateLines[i].replace(propertyRegEx, placeHolder);
                    }
                }
            }

            var partialTemplates = openTemplates.filter(template => template.parent !== null);

            var partialTemplatesMap = {};
            partialTemplates.forEach(template => partialTemplatesMap[template.name] = template.data);

            reqBody.templateBase64 = btoa(templateLines.join('\n'));
            reqBody.templatesMapBase64 = btoa(JSON.stringify(partialTemplatesMap));
            reqBody.replacementDictionaryBase64 = btoa(JSON.stringify(replacementDictionary));
        }

        latestRequest++;
        const requestNumber = latestRequest;
        $.ajax('/api/convert/hl7?api-version=' + version, {
            'data': JSON.stringify(reqBody),
            'type': 'POST',
            'processData': false,
            'contentType': 'application/json',
            'beforeSend': function (request) {
                request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
            },
            'success': function (data) {
                if (latestRequest === requestNumber) {
                    // Formatting output and splitting on line breaks (taking platform variations into consideration)
                    outputLines = JSON.stringify(data.fhirResource, null, 2).replace(/(?:\r\n|\r|\n)/g, '\n').split('\n');

                    // Reset line mappings
                    if (activeTemplate === topTemplate && getSettings().scrollSync) {
                        lineMapping = [];
                        lineMapping.push({ source: 0, destination: 0 });
                        if (templateLines.length > 1 && outputLines.length > 1) {
                            lineMapping.push({ source: templateLines.length - 1, destination: outputLines.length - 1 });
                        }

                        // Loop through lines and undo substitutions while building line mapping
                        var substitutionRegEx = /"\$([0-9]+)\$":/;
                        for (var i = 0; i < outputLines.length; i++) {
                            var subMatch = outputLines[i].match(substitutionRegEx);
                            if (subMatch && subMatch.length > 0) {
                                lineMapping.push({ source: Number(subMatch[1]), destination: i });
                                outputLines[i] = outputLines[i].replace(substitutionRegEx, replacementDictionary[subMatch[0]]);
                            }
                        }
                    }

                    outputCode.setValue(outputLines.join('\n'));
                    if (!resetOutputScroll) {
                        setSkipNextScrollHandler(outputCode, true);
                        outputCode.scrollTo(null, scrollInfo.top);
                    }

                    // Makes the unused line marking asyc to help performance.
                    /*setTimeout(() => {
                        if (latestRequest === requestNumber) {
                            // Highlights unused sections of the Hl7 message
                            var unusedReport = data.unusedSegments;
                            var messageDoc = messageEditor.getDoc();

                            var fieldSeparator = messageDoc.getLine(0)[3];
                            var componentSeparator = messageDoc.getLine(0)[4];

                            // Removes all the old highlighting
                            messageDoc.getAllMarks().forEach((mark) => mark.clear());

                            unusedReport.forEach((line) => {
                                line.field.forEach((field) => {
                                    if (field && field.index !== 0 && field.component.length > 0) {
                                        field.component.forEach((component) => {
                                            var lineText = messageDoc.getLine(line.line);
                                            var startFieldIndex = indexOfX(lineText, fieldSeparator, field.index - 1) + 1;
                                            var endFieldIndex = indexOfX(lineText, fieldSeparator, field.index);
                                            if (endFieldIndex === -1) {
                                                endFieldIndex = lineText.length;
                                            }

                                            var startComponentIndex = indexOfX(lineText.substring(startFieldIndex, endFieldIndex), componentSeparator, component.index - 1) + startFieldIndex + 1;
                                            var endComponentIndex = startComponentIndex + component.value.length;
                                            messageDoc.markText({ line: line.line, ch: startComponentIndex }, { line: line.line, ch: endComponentIndex }, { className: 'unused-segment' });
                                        });
                                    }
                                });
                            });
                        }
                    }, 0);*/
                }

            },
            'error': function (err) {
                try {
                    var errObj = JSON.parse(err.responseText);
                    outputCode.setValue(`{${errObj.error.code}: ${errObj.error.message}}`);
                }
                catch (ex) {
                    outputCode.setValue('Unable to convert: ' + JSON.stringify(err));
                }
            }
        });
    }, waitForTypingTimeout);
}

function indexOfX(source, target, number, offset = 0) {
    if (offset === -1 || number < 0) {
        return -1;
    }
    else if (number === 0) {
        return source.indexOf(target, offset);
    }

    return indexOfX(source, target, number - 1, source.indexOf(target, offset) + 1);
}

function loadMessage(messageFile) {
    $.get("api/messages/" + messageFile + '?code=' + getApiKey(), function (data) {
        messageEditor.setValue(data);
        convertMessage();
    });
}

function loadTemplate(templateFile) {
    var okToLoadTemplate;

    // Compare to cache while making sure line breaks are handled consistently
    if (openTemplates.reduce(
        (changesNotFound, template) => unchangedFromReference(template.name, template.data) && changesNotFound,
        true)) {
        okToLoadTemplate = true;
    } else {
        okToLoadTemplate = confirm('You have unsaved changes, loading template will reset changes.');
    }

    if (okToLoadTemplate) {
        openTemplates.forEach(template => closeTab(template.name, true));
        openTemplates = [];
        currentTemplateReference = {};
        addTab(templateFile, null);
    }
}

function saveTemplate() {
    var reqBody = templateCodeEditor.getValue();
    var templateName = $('#template-name-input').val();

    $.ajax('/api/templates/' + templateName, {
        'data': reqBody,
        'type': 'PUT',
        'processData': false,
        'contentType': 'text/plain',
        'beforeSend': function (request) {
            request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
        },
        'success': function () {
            var oldName = activeTemplate.name;

            if (oldName !== templateName) {
                refreshTemplateNames();
                $('#template-name-input').val(templateName);

                renameTab(oldName, templateName);
            }

            currentTemplateReference[templateName] = reqBody;

            getTab(activeTemplate.name).removeClass("font-italic");
            displayBanner("Save successful", "alert-success");
        },
        'error': function (response) {
            try {
                console.error(response.responseJSON.error.message);
                displayBanner(response.responseJSON.error.message, "alert-danger");
            }
            catch (ex) {
                console.error('Error saving template: ' + JSON.stringify(response));
                displayBanner('Error saving template: ' + JSON.stringify(response), "alert-danger");
            }
        }
    });
}

function loadMessageOptions() {
    $.getJSON('/api/messages' + '?code=' + getApiKey(), function (messageList) {
        $("#message-load-dropdown").html('');

        $.each(messageList.messages, function (index, item) {
            $("#message-load-dropdown").append("<a class=\"dropdown-item\" href=\"#\">" + item.messageName + "</a>");
        });
    });
}

function loadTemplateOptions() {
    $.getJSON('/api/templates?code=' + getApiKey(), function (templateList) {
        templateNames = templateList.templates.map(template => template.templateName);
        $("#template-load-dropdown").html('');
        $.each(templateList.templates, function (index, item) {
            addToTemplateDropdown("#template-load-dropdown", item.templateName, item.templateName, 0);
        });
    });
}

function refreshTemplateNames() {
    $.getJSON('/api/templates?code=' + getApiKey(), function (templateList) {
        templateNames = templateList.templates.map(template => template.templateName);
    });
}

function addToTemplateDropdown(menuId, fullPath, templateName, level) {
    const baseMenuId = "template-load-dropdown-submenu-";
    const toggleId = "-toggle";
    const containerId = "-container";

    if (templateName.includes("/")) {
        var parent = templateName.split("/", 1)[0];
        templateName = templateName.substring(parent.length + 1);
        var childMenuId = "#" + baseMenuId + parent + "-" + level;

        var childMenu = $(childMenuId);
        if (childMenu.length === 0) {
            $(menuId).append(
                "<div id=\"" + childMenuId.substring(1) + containerId + "\" class=\"nav-item dropright\" role=\"menu\">"
                + "<a id=\"" + childMenuId.substring(1) + toggleId + "\" class=\"dropdown-item dropdown-toggle font-weight-bold\" href=\"#\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">" + parent + "</a>"
                + "<div id=\"" + childMenuId.substring(1) + "\" class=\"dropdown-menu ml-0\"></div>"
                + "</div>");

            $(childMenuId + containerId).on("mouseenter", () => {
                $(childMenuId).addClass("show");
            });

            $(childMenuId + containerId).on("mouseleave", () => {
                $(childMenuId).removeClass("show");
            });

            $(childMenuId + toggleId).on("focus", () => {
                $(childMenuId).addClass("show");
            });

            $(childMenuId + toggleId).on("click", (e) => {
                $(this).next('div').toggle();
                e.stopPropagation();
                e.preventDefault();
            });
        }

        addToTemplateDropdown(childMenuId, fullPath, templateName, level + 1);
    }
    else {
        $(menuId).append("<a class=\"dropdown-item\" href=\"#\" onClick=\"loadTemplate('" + fullPath + "');\">" + templateName + "</a>");
    }
}

function loadGitMenu() {
    $.getJSON('/api/templates/git/status?code=' + getApiKey(), function (status) {
        $.getJSON('/api/templates/git/branches?code=' + getApiKey(), function (branches) {
            $("#git-dropdown").html('');
            if (status.length > 0) {
                $("#git-dropdown").append("<a class=\"dropdown-item commit-link\" href=\"#\">Commit changes</a>");
                $("#git-dropdown").on('click', 'a.commit-link', function () {
                    commitChanges();
                });
            } else {
                $("#git-dropdown").append("<a class=\"dropdown-item disabled\">Commit changes (none)</a>");
            }
            $("#git-dropdown").append("<a class=\"dropdown-item\" data-toggle=\"modal\" data-target=\"#new-branch-modal\" href=\"#\">New branch</a>");
            $("#git-dropdown").append("<div class=\"dropdown-divider\"></div>");
            $("#git-dropdown").append("<h6 class=\"dropdown-header\">Branches</h6>");
            branches.forEach(function (b) {
                if (b.active) {
                    $("#git-dropdown").append("<a class=\"dropdown-item\" href=\"#\"><strong>" + b.name + "<strong></a>");
                } else {
                    $("#git-dropdown").append("<a class=\"dropdown-item\" href=\"#\" onClick=\"checkoutBranch('" + b.name + "');\">" + b.name + "</a>");
                }
            });
        });
    });
}

function loadBaseBranches() {
    $.getJSON('/api/templates/git/branches?code=' + getApiKey(), function (branches) {
        $('#base-branch-select').find('option').remove().end();
        branches.forEach(function (b) {
            $("#base-branch-select").append(new Option(b.name, b.name, null, b.active));
        });
    });
}

function createBranch(branchName, baseBranchName, checkoutBranchAfterCreate) {
    $('#new-branch-modal').modal('hide');
    $.ajax('/api/templates/git/branches', {
        'data': JSON.stringify({ name: branchName, baseBranch: baseBranchName }),
        'type': 'POST',
        'processData': false,
        'contentType': 'application/json',
        'beforeSend': function (request) {
            request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
        },
        'success': function () {
            loadGitMenu();
            if (checkoutBranchAfterCreate) {
                checkoutBranch(branchName);
            }
        },
        'error': function (err) {
            console.error('Error creating branch: ' + JSON.stringify(err));
            displayBanner('Error creating branch: ' + JSON.stringify(err), 'alert-danger');
        }
    });
}

function checkoutBranch(branchName) {
    $.ajax('/api/templates/git/checkout', {
        'data': JSON.stringify({ name: branchName }),
        'type': 'POST',
        'processData': false,
        'contentType': 'application/json',
        'beforeSend': function (request) {
            request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
        },
        'success': function () {
            refreshTemplateNames();
            loadGitMenu();
            if ($('#template-name-input').val()) {
                loadTemplate($('#template-name-input').val());
            }
        },
        'error': function (err) {
            console.error('Error checking out branch: ' + JSON.stringify(err));
            displayBanner('Error checking out branch: ' + JSON.stringify(err), 'alert-danger');
        }
    });
}

function commitChanges() {
    $.ajax('/api/templates/git/commit', {
        'type': 'POST',
        'processData': false,
        'contentType': 'application/json',
        'beforeSend': function (request) {
            request.setRequestHeader("X-MS-CONVERSION-API-KEY", getApiKey());
        },
        'success': function () {
            refreshTemplateNames();
            loadGitMenu();
        },
        'error': function (err) {
            console.error('Error commiting changes: ' + JSON.stringify(err));
            displayBanner('Error commiting changes: ' + JSON.stringify(err), 'alert-danger');
        }
    });
}

function setColorTheme(isDarkMode) {
    var addColor = isDarkMode ? "dark" : "light";
    var removeColor = isDarkMode ? "light" : "dark";

    var body = $('body');
    body.removeClass("bg-" + removeColor);
    body.addClass("bg-" + addColor);

    var navbar = $('#navbar');
    navbar.removeClass("bg-" + removeColor);
    navbar.removeClass("navbar-" + removeColor);

    navbar.addClass("bg-" + addColor);
    navbar.addClass("navbar-" + addColor);

    var templateTabs = $('#template-tabs');
    templateTabs.removeClass("bg-" + removeColor);
    navbar.addClass("bg-" + addColor);

    var buttons = [$('#template-save-button'), $('#refresh-button'), $('#settings-save-button')];
    buttons.forEach((btn) => {
        // buttons are inverted to stand out
        btn.removeClass("btn-" + addColor);
        btn.addClass("btn-" + removeColor);
    });

    var settingsModal = $('#settings-modal-content');
    settingsModal.removeClass("bg-" + removeColor);
    settingsModal.removeClass("text-" + addColor);

    settingsModal.addClass("bg-" + addColor);
    settingsModal.addClass("text-" + removeColor);

    messageEditor.setOption("theme", isDarkMode ? darkMode : lightMode);
    outputCode.setOption("theme", isDarkMode ? darkMode : lightMode);
    templateCodeEditor.setOption("theme", isDarkMode ? darkMode : lightMode);
}

function displayBanner(message, cssClass) {
    var delay = 0;
    var banner = $('#banner');

    if (bannerFade) {
        clearTimeout(bannerFade);
        banner.slideUp(transitionTime);
        delay = transitionTime;
    }

    bannerFade = setTimeout(() => {
        $('#banner-message')[0].innerText = message;
        banner.removeClass();
        banner.addClass('alert ' + cssClass);
        banner.slideDown(transitionTime);

        bannerFade = setTimeout(() => {
            banner.slideUp(transitionTime);
            bannerFade = null;
        }, 5000);
    }, delay);
}

function isTemplateName(templateName) {
    return templateNames.includes(templateName);
}

function addTab(templateName, parentName) {
    if (!openTemplates.reduce((isOpen, template) => template.name === templateName ? true : isOpen, false)) {
        $.get("api/templates/" + templateName + '?code=' + getApiKey(), function (data) {
            currentTemplateReference[templateName] = data;

            var tabs = $('#template-tabs');
            var tabHtml = "<a class=\"nav-item nav-link\" onclick=\"changeTab('" + templateName + "')\">" + templateName;
            if (parentName) {
                tabHtml += "<button type=\"button\" class=\"close\" onclick=\"closeTab('" + templateName + "')\"><span>&times;</span></button>";
            }
            tabHtml += "</a>";
            tabs.append(tabHtml);

            openTemplates.push({ name: templateName, parent: parentName, data: data, active: true, marks: [] });
            changeTab(templateName);
        });
    }
    else {
        changeTab(templateName);
    }
}

function changeTab(templateName) {
    var templateObj = openTemplates.find(template => template.name === templateName);

    if (templateObj) {
        var oldTab = getTab(activeTemplate.name);
        if (oldTab) {
            oldTab.removeClass('font-weight-bold');
        }

        openTemplates.forEach(template => template.active = false);
        templateObj.active = true;
        activeTemplate = templateObj;

        templateCodeEditor.setValue(templateObj.data);

        // This can be made more efficent. It shouldn't be nessisary to recheck the whole document after every load.
        underlinePartialTemplateNames(templateCodeEditor.getDoc());

        $('#template-name-input').val(templateName);

        getTab(templateName).addClass('font-weight-bold');
    }
}

function closeTab(templateName, force = false) {
    var canCloseTab = force;
    if (!force) {
        if (unchangedFromReference(templateName, openTemplates.find(template => template.name === templateName).data)) {
            canCloseTab = true;
        } else {
            canCloseTab = confirm('You have unsaved changes, closing template will lose changes.');
        }
    }

    if (canCloseTab) {
        var tabs = $('#template-tabs')[0];
        for (var tab of tabs.children) {
            if (tab.innerText.includes(templateName)) {
                tabs.removeChild(tab);
                break;
            }
        }

        if (activeTemplate.name === templateName && activeTemplate.parent) {
            changeTab(activeTemplate.parent);
        }

        var parent = openTemplates.find(template => template.name === templateName).parent;
        openTemplates.forEach(template => {
            if (template.parent === templateName) {
                template.parent = parent;
            }
        });

        openTemplates = openTemplates.filter(template => template.name !== templateName);
        delete currentTemplateReference[templateName];
    }
}

function renameTab(oldName, newName) {
    var tabs = $('#template-tabs')[0];
    for (var tab of tabs.children) {
        if (tab.innerText.includes(oldName)) {
            var regex = new RegExp(oldName, 'ig');
            tab.outerHTML = tab.outerHTML.replace(regex, newName);
        }
    }

    openTemplates.find(template => template.name === oldName).name = newName;
    openTemplates.forEach(template => {
        if (template.parent === oldName) {
            template.parent = newName;
        }
    });
}

function getTab(templateName) {
    var tabs = $('#template-tabs')[0];
    for (var tab of tabs.children) {
        if (tab.innerText.includes(templateName)) {
            return $(tab);
        }
    }
}

function underlinePartialTemplateNames(document, change) {
    var startLine = change ? change.from.line : document.firstLine();
    var endLine = (change ? change.to.line : document.lastLine()) + 1;

    // remove marks before adding new one, only check changed lines if passed
    activeTemplate.marks = activeTemplate.marks.filter(mark => {
        var section = mark.find();
        if (section) {
            if (section.from.line >= startLine && section.to.line < endLine) {
                mark.clear();
                return false;
            }
        }
        else {
            return false;
        }

        return true;
    });

    document.eachLine(startLine, endLine, (line) => {
        var nameLocation = getPartialTemplateNameLocation(line.text);
        if (nameLocation) {
            activeTemplate.marks.push(
                document.markText(
                    {
                        line: line.lineNo(),
                        ch: nameLocation.start
                    },
                    {
                        line: line.lineNo(),
                        ch: nameLocation.end
                    },
                    {
                        className: "underline"
                    }));
        }
    });
}

function getPartialTemplateNameLocation(lineText) {
    var startPoint = lineText.indexOf("{{>");

    if (startPoint !== -1) {
        var nextWhitespace = lineText.indexOf(" ", startPoint);
        var handlebarEnd = lineText.indexOf("}}", startPoint);
        var endPoint = nextWhitespace < handlebarEnd && nextWhitespace !== -1 ? nextWhitespace : handlebarEnd;

        if (endPoint !== -1) {
            var templateName = lineText.substring(startPoint + 3, endPoint);
            if (isTemplateName(templateName)) {
                return { start: startPoint + 3, end: endPoint, name: templateName };
            }
        }
    }

    return null;
}

function unchangedFromReference(templateName, templateData) {
    var newLineRegex = /(?:\r\n|\r|\n)/g;

    return templateData.replace(newLineRegex, '\n') === currentTemplateReference[templateName].replace(newLineRegex, '\n');
}

$(document).ready(function () {
    messageEditor = CodeMirror.fromTextArea(document.getElementById("hl7messagebox"), {
        //readOnly: false,
        lineNumbers: true,
        theme: lightMode,
        mode: { name: "text/html" },
        extraKeys: { "Ctrl-Q": function (cm) { cm.foldCode(cm.getCursor()); } },
        foldGutter: true,
        gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"]
    });

    messageEditor.on("change", function () {
        convertMessage();
    });

    outputCode = CodeMirror.fromTextArea(document.getElementById("resultbox"), {
        readOnly: true,
        lineNumbers: true,
        theme: lightMode,
        mode: { name: "javascript" },
        extraKeys: { "Ctrl-Q": function (cm) { cm.foldCode(cm.getCursor()); } },
        foldGutter: true,
        gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"]
    });

    templateCodeEditor = CodeMirror.fromTextArea(document.getElementById("templatebox"), {
        theme: lightMode,
        lineNumbers: true,
        /*global hintExtraKeysObj*/
        extraKeys: hintExtraKeysObj,
        mode: { name: "handlebars", base: "application/json" },
        smartIndent: false,
        matchBrackets: true
    });

    templateCodeEditor.on("change", function (instance, changeObj) {
        if (activeTemplate) {
            activeTemplate.data = templateCodeEditor.getValue();
            underlinePartialTemplateNames(templateCodeEditor.getDoc(), changeObj);

            if (unchangedFromReference(activeTemplate.name, activeTemplate.data)) {
                getTab(activeTemplate.name).removeClass("font-italic");
            }
            else {
                getTab(activeTemplate.name).addClass("font-italic");
            }
        }
        convertMessage();
    });

    templateCodeEditor.on('scroll', function () {
        if (activeTemplate.parent === null) {
            adjustScrolling(templateCodeEditor, outputCode, lineMapping.map(e => { return e.source; }), lineMapping.map(e => { return e.destination; }));
        }
    });

    templateCodeEditor.on('dblclick', function () {
        var doc = templateCodeEditor.getDoc();
        var cursorPos = doc.getCursor();
        var lineText = doc.getLine(cursorPos.line);
        var nameLocation = getPartialTemplateNameLocation(lineText);

        if (nameLocation && cursorPos.ch >= nameLocation.start && cursorPos.ch <= nameLocation.end) {
            addTab(nameLocation.name, activeTemplate.name);
        }
    });

    outputCode.on('scroll', function () {
        if (activeTemplate.parent === null) {
            adjustScrolling(outputCode, templateCodeEditor, lineMapping.map(e => { return e.destination; }), lineMapping.map(e => { return e.source; }));
        }
    });

    $('#template-dropdown-button').on('click', function () {
        loadTemplateOptions();
    });

    $('#message-dropdown-button').on('click', function () {
        loadMessageOptions();
    });

    $("#message-load-dropdown").on('click', 'a', function () {
        loadMessage($(this).text());
    });

    $('#git-dropdown-button').on('click', function () {
        loadGitMenu();
    });

    //Template save button
    $('#template-save-button').on('click', function () {
        saveTemplate();
    });

    $('#banner-close').on('click', function () {
        $('#banner').slideUp(transitionTime);
        clearTimeout(bannerFade);
        bannerFade = null;
    });

    $("#settings-modal").on('show.bs.modal', function () {
        uncheckedApiKey = true;
        $('#api-key-input').val(!hasSuccessfulCallBeenMade || apiKey === '' ? '' : maskedApiKey);
        $('#settings-scroll-sync').prop('checked', getSettings().scrollSync);
        $('#settings-dark-mode').prop('checked', getSettings().darkMode);
    });

    $('#api-key-input').on('input', function () {
        $('#settings-modal-alert').hide();
    });

    // We will not allow the API key modal to be closed if the API key is wrong. 
    $("#settings-modal").on('hidden.bs.modal', function () {
        if (uncheckedApiKey) {
            checkApiKey(undefined, function () { $('#settings-modal').modal('show'); });
        }
    });

    // API Key save button 
    $('#settings-save-button').on('click', function () {
        var settings = getSettings();
        var changesRequireNewConversion = false;
        var changesColorTheme = false;

        if (settings.scrollSync != $('#settings-scroll-sync').prop('checked')) {
            changesRequireNewConversion = true;
        }

        if (settings.darkMode != $('#settings-dark-mode').prop('checked')) {
            changesColorTheme = true;
        }

        settings.scrollSync = $('#settings-scroll-sync').prop('checked');
        settings.darkMode = $('#settings-dark-mode').prop('checked');

        var apiKeyInput = $('#api-key-input').val();
        apiKey = apiKeyInput === maskedApiKey ? apiKey : apiKeyInput;

        setSettings(settings);
        checkApiKeyAndRaiseModal(true);

        if (changesRequireNewConversion) {
            convertMessage();
        }

        if (changesColorTheme) {
            setColorTheme(settings.darkMode);
        }
    });

    $('#refresh-button').on('click', function () {
        convertMessage();
    });

    $("#new-branch-modal").on('show.bs.modal', function () {
        loadBaseBranches();
    });

    // New branch save button 
    $('#new-branch-create-button').on('click', function () {
        createBranch($('#branch-name-input').val(), $('#base-branch-select').val(), $("#checkout-new-branch").prop("checked") == true);
    });

    // Create splits for editor areas
    Split(['.template-area', '.output-area'], {
        gutterSize: 5,
        sizes: [50, 50]
    });

    Split(['.msg-area', '.editor-area'], {
        gutterSize: 5,
        sizes: [30, 70]
    });

    // See if we have a valid API key and if not raise modal
    checkApiKeyAndRaiseModal();
});
