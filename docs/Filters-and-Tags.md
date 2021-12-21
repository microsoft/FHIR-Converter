# Filters and Tags

⚠ **This document applies to the Liquid engine. Follow [this](https://github.com/microsoft/FHIR-Converter/tree/handlebars) link for the documentation of Handlebars engine.**

## Filters

By default, Liquid provides a set of [standard filters](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers#standard-filters) to assist template creation.
Besides these filters, FHIR Converter also provides some other filters that are useful in conversion, which are listed below.
If these filters do not meet your needs, you can also write your own filters.

### Hl7v2 specific filters
| Filter | Description | Syntax |
|-|-|-|
| get_first_segments | Returns first instance of the segments | `{% assign result = hl7v2Data \| get_first_segments: 'segment1\|segment2\|...' -%}` |
| get_segment_lists | Extracts HL7 v2 segments | `{% assign result = hl7v2Data \| get_segment_lists: 'segment1\|segment2\|...' -%}` |
| get_related_segment_list | Given a segment and related segment name, returns the collection of related named segments | `{% assign result = hl7v2Data \| get_related_segment_list: parentSegment, 'childSegmentName' -%}` |
| get_parent_segment | Given a child segment name and overall message index, returns the first matched parent segment | `{% assign result = hl7v2Data \| get_parent_segment: 'childSegmentName', 3, 'parentSegmentName' -%}` |
| has_segments | Checks if HL7 v2 message has segments | `{% assign result = hl7v2Data \| has_segments: 'segment1\|segment2\|...' -%}` | 
| split_data_by_segments | Given an HL7 v2 message and segment name(s) as the separator(s), returns the message list split by separator(s). <br> Note: Each segment separator will be retained as the first segment of each message in the list, while the segments before the first separator (which may be empty) will be retained as the first message in the list without any separator. |  `{% assign result = hl7v2Data \| split_data_by_segments: 'segment1\|segment2\|...' -%}` | 

### C-CDA specific filters
| Filter | Description | Syntax |
|-|-|-|
| get_first_ccda_sections | Returns first instance (non-alphanumeric chars replace by '_' in name) of the sections | `{% assign firstSections = msg \| get_first_ccda_sections: 'Problems' -%}` |
| get_ccda_section_lists | Returns instance list (non-alphanumeric chars replace by '_' in name) for the given sections | `{% assign sections = msg \| get_ccda_section_lists: 'Problems' -%}` |
| get_first_ccda_sections_by_template_id | Returns first instance (non-alphanumeric chars replace by '_' in name) of the sections by template id | `{% assign firstSections = msg \| get_first_ccda_sections_by_template_id: '2.16.840.1.113883.10.20.22.2.5.1' -%}` |

### String Filters
| Filter | Description | Syntax |
|-|-|-|
| char_at | Returns char at position index | `{{ 'foo' \| char_at: 0 }} #=> 'f'` |
| contains | Returns true if a string includes another string | `{{ 'foo' \| contains: 'fo' }} #=> true` |
| escape_special_chars | Returns string with special chars escaped | `{{ '\E' \| escape_special_chars }} #=> '\\E'` |
| unescape_special_chars | Returns string after removing escaping of special char | `{{ '\\E' \| unescape_special_chars }} #=> '\E'` |
| match | Returns an array containing matches with a regular expression | `{% assign m = code \| match: '[0123456789.]+' -%}` |
| to_json_string | Converts to JSON string | `{% assign msgJsonString = msg \| to_json_string -%}` |
| to_double | Converts string to double | `{{ "100.01" \| to_double }} ` |
| base64_encode | Returns base64 encoded string | `{{ decodedData \| base64_encode }}` |
| base64_decode | Returns base64 decoded string | `{{ encodedData \| base64_decode }}` |
| sha1_hash | Returns SHA1 hash (in hex) of given string | `{{ inputData \| sha1_hash }}` |
| gzip | Returns compressed string | `{{ uncompressedData \| gzip }}` |
| gunzip_base64_string | Returns decompressed string | `{{ compressedData \| gunzip_base64_string }}` |

### Math filters
| Filter | Description | Syntax |
|-|-|-|
| is_nan | Checks if the object is not a number | `{{ true \| is_nan }} #=> true` |
| abs | Returns the absolute value of a number | `{{ -2019.6 \| abs }} #=> 2019.6` |
| pow | Returns the base to the exponent power, that is, base^exponent | `{{ 3 \| pow: 3 }} #=> 27` |
| random | Returns a non-negative random integer that is less than the specified maximum. | `{{ 100 \| random }} #=> 52` |
| sign | Returns either a positive or negative +/- 1, indicating the sign of a number passed into the argument. If the number passed into is 0, it will return a 0. Note that if the number is positive, an explicit (+) will not be returned | `{{ -5 \| sign }} #=> -1` | 
| truncate_number | Returns the integer part of a number by removing any fractional digits | `{{ -34.53 \| truncate_number }} #=> -34` |
| divide | Divides first number by the second number and return a double | `{{ 5 \| divide: 2 }} #=> 2.5` | 

### DateTime filters
| Filter | Description | Syntax |
|-|-|-|
| add_hyphens_date | Adds hyphens to a date without hyphens | `{{ PID.7.Value \| add_hyphens_date }}` |
| format_as_date_time | Converts an YYYYMMDDHHmmssSSS string (e.g. 20040629175400000) to a dateTime format (e.g. 2004-06-29T17:54:00.000z). Provides parameters to handle time zone: `preserve`, `utc`, `local`. The default method is `local`. | `{{ PID.29.Value \| format_as_date_time: 'utc' }}` |
| now | Provides current time in a specific format. The default format is "yyyy-MM-ddTHH:mm:ss.FFFZ". | `{{ '' \| now: 'dddd, dd MMMM yyyy HH:mm:ss' }}` |
| add_seconds | Adds double seconds on datetime in FHIR format. Provides parameters to set time zones: `preserve`, `utc`, `local`. The default is set to the parameter `preserve`. | `{{ "2021-01-01T00:00:00Z" \| add_seconds:100.1 }}` |

### Collection filters
| Filter | Description | Syntax |
|-|-|-|
| to_array | Returns an array created (if needed) from given object | `{% assign authors = msg.ClinicalDocument.author \| to_array -%}` |
| concat | Returns the concatenation of provided arrays | `{% assign ethnicCodes = ethnicCodes1 \| concat: ethnicCodes2 -%}` |
| batch_render | Render every entry in a collection with a snippet and a variable name set in snippet | `{{ firstSections.2_16_840_1_113883_10_20_22_2_5_1.entry \| to_array \| batch_render: 'Entry/Problem/entry', 'entry' }}` |

### Miscellaneous filters
| Filter | Description | Syntax |
|-|-|-|
| get_property | Returns a specific property of a coding with mapping file [CodeSystem.json](../data/Templates/Hl7v2/CodeSystem/CodeSystem.json) | `{{ PID.8.Value \| get_property: 'CodeSystem/Gender', 'code' }}` |
| generate_uuid | Generates an ID based on an input string | `{% assign patientId = firstSegments.PID.3.1.Value \| generate_uuid -%}` |
| generate_id_input | Generates an input string for generate_uuid with 1) the resource type, 2) whether a base ID is required, 3) the base ID (optional) | `{{ identifiers \| generate_id_input: 'Observation', false, baseId \| generate_uuid }}` |

## Tags

By default, Liquid provides a set of standard [tags](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers#tags) to assist template creation. Besides these tags, FHIR Converter also provides some other tags that are useful in conversion, which are listed below. If these tags do not meet your needs, you can also write your own tags.

| Tag | Description | Syntax |
|-|-|-|
| evaluate | Evaluates an ID with an ID generation template and input data | `{% evaluate patientId using 'Utils/GenerateId' obj:msg.ClinicalDocument.recordTarget.patientRole -%}` |
