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
| add_hyphens_date | Adds hyphens to a date or a partial date that does not have hyphens to make it into a valid FHIR format. The input date format is YYYY, YYYYMM, or YYYYMMDD. The output format is a valid FHIR date or a partial date format: YYYY, YYYY-MM, or YYYY-MM-DD.  | `{{ PID.7.Value \| add_hyphens_date }}` |
| format_as_date_time | Converts valid HL7v2 and C-CDA datetime to a valid FHIR datetime format. The input datetime format is datetime or partial datetime without hyphens: YYYY[MM[DD[HH[MM[SS[.S[S[S[S]]]]]]]]][+/-ZZZZ]. For example, the input 20040629175400000 will have the output 2004-06-29T17:54:00.000Z. Provides parameters to handle different time zones: preserve, utc, local. The default method is preserve. | `{{ PID.29.Value \| format_as_date_time: 'utc' }}` |
| now | Provides the current time in a specific format. The default format is yyyy-MM-ddTHH:mm:ss.FFFZ. | `{{ '' \| now: 'dddd, dd MMMM yyyy HH:mm:ss' }}` |
| add_seconds | Adds double seconds on a valid [FHIR datetime](http://hl7.org/fhir/R4/datatypes.html) (e.g.2021-01-01T00:00:00Z). Provides parameters to handle different time zones: preserve, utc, local. The default method is preserve. | `{{ "2021-01-01T00:00:00Z" \| add_seconds:100.1, 'utc' }}` |

DateTime filters usage examples:

- add_hyphens_date
```
    {{ "2001" | add_hyphens_date }} -> 2001
    {{ "200101" | add_hyphens_date }} -> 2001-01
    {{ "20010101" | add_hyphens_date }} -> 2001-01-01
```

- format_as_date_time
```
    {{ "20110103143428-0800" | format_as_date_time }} -> 2011-01-03T14:34:28-08:00
    {{ "20110103143428-0800" | format_as_date_time: 'preserve' }} -> 2011-01-03T14:34:28-08:00
    {{ "20110103143428-0800" | format_as_date_time: 'utc' }} -> 2011-01-03T22:34:28Z
```
>[Note] : `format_as_date_time` and `add_hyphens_date` are used to convert HL7v2 and C-CDA date and datetime format to FHIR. For other date or datetime's reformat, please refer to the standard filter [date](https://shopify.dev/api/liquid/filters/additional-filters#date).
- now
```
    {{ "" | now  }} -> 2022-03-22T06:50:25.071Z // an example time
    {{ "" | now: 'dddd, dd MMMM yyyy HH:mm:ss' }} -> Tuesday, 22 March 2022 06:52:15
    {{ "" | now: 'd' }} -> 3/22/2022
```
> [Note] : Input string will not affect the result.
- add_seconds
```
    {{ "1970-01-01T00:01:00.000+10:00" | add_seconds: -60, 'utc' }} -> 1969-12-31T14:00:00.000Z
    {{ "1970-01-01T00:01:00Z" | add_seconds: 60.1230 }} -> 1970-01-01T00:02:00.123Z
    {{ "1970-01-01T00:01:00+14:00" | add_seconds: 60, 'preserve' }} -> 1970-01-01T00:01:01+14:00
```

> [Note] : If the input is a partial datetime without time zone, it will be set to the first day of the year and 00:00:00 clock time with local time zone as suffix. e.g. In the location with +08:00 time zone, the input string "201101031434" will be filled to "20110103143400+0800". The template {{ "201101031434" | format_as_date_time: 'utc'}} will output 2011-01-03T06:34:00Z when running at +08:00 location.


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
