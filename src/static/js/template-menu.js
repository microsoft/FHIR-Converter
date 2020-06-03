function loadMessage(messageFile) {
    $.get(getUrl('messages', messageFile), function (data) {
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

    $.ajax(getUrl('templates', templateName), {
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
    $.getJSON(getUrl('messages'), function (messageList) {
        $("#message-load-dropdown").html('');

        $.each(messageList.messages, function (index, item) {
            $("#message-load-dropdown").append("<a class=\"dropdown-item\" href=\"#\">" + item.messageName + "</a>");
        });
    });
}

function loadTemplateOptions() {
    $.getJSON(getUrl('templates'), function (templateList) {
        templateNames = templateList.templates.map(template => template.templateName);
        $("#template-load-dropdown").html('');
        $.each(templateList.templates, function (index, item) {
            addToTemplateDropdown("#template-load-dropdown", item.templateName, item.templateName, 0);
        });
    });
}

function refreshTemplateNames() {
    $.getJSON(getUrl('templates'), function (templateList) {
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

function isTemplateName(templateName) {
    return templateNames.includes(templateName);
}