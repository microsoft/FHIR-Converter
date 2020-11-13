# Filters


⚠ **This document applies to the Liquid engine. Follow [this](https://github.com/microsoft/FHIR-Converter/tree/handlebars) link for the documentation of Handlebars engine.**

By default, Liquid provides a set of [standard filters](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers#standard-filters) to assist template creation.
Besides these filters, FHIR Converter also provides some other filters that are useful in conversion, which are listed below.
If these filters do not meet your needs, you can also write your own filters.

### Hl7 v2 specific filters
| Filter | Description | Syntax |
|-|-|-|
| get_first_segments | Returns first instance of the segments | `{% assign result = hl7v2Data \| get_first_segments: 'segment1\|segment2\|...' -%}` |
| get_segment_lists | Extracts HL7 v2 segments | `{% assign result = hl7v2Data \| get_segment_lists: 'segment1\|segment2\|...' -%}` |
| get_related_segment_list | Given a segment and related segment name, returns the collection of related named segments | `{% assign result = hl7v2Data \| get_related_segment_list: parentSegment, 'childSegmentName' -%}` |
| get_parent_segment | Given a child segment name and overall message index, returns the first matched parent segment | `{% assign result = hl7v2Data \| get_parent_segment: 'childSegmentName', 3, 'parentSegmentName' -%}` |
| has_segments | Checks if HL7 v2 message has segments | `{% assign result = hl7v2Data \| has_segments: 'segment1\|segment2\|...' -%}` | 

### String Filters
| Filter | Description | Syntax |
|-|-|-|
| char_at | Returns char at position index | `{{ 'foo' \| char_at: 0 }} #=> 'f'` |
| contains | Returns true if a string includes another string | `{{ 'foo' \| contains: 'fo' }} #=> true` |
| escape_special_chars | Returns string with special chars escaped | `{{ '\E' \| escape_special_chars }} #=> '\\E'` |
| unescape_special_chars | Returns string after removing escaping of special char | `{{ '\\E' \| unescape_special_chars }} #=> '\E'` |

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
| format_as_date_time | Convert an YYYYMMDDHHmmssSSS string, e.g. 20040629175400000 to dateTime format, e.g. 2004-06-29T17:54:00.000z | `{{ PID.29.Value \| format_as_date_time }}` |

### Miscellaneous filters
| Filter | Description | Syntax |
|-|-|-|
| get_property | Returns a specific property of code system with mapping file | `{{ PID.8.Value \| get_property: 'CodeSystem/Gender', 'code' }}` |
| evaluate | Returns a specific property of code system with template | `{{ input \| evaluate: 'code' }}` |
| generate_uuid | Generates an ID based on input object, which can be a segment, field, component or string in HL7 v2 | `{% assign patientId = firstSegments.PID.3.1.Value \| generate_uuid -%}` |
