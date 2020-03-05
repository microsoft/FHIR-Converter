# Helper Functions

The open-source release includes a set of helper functions to assist with template creation. The current list of available helper functions is below. If these do not meet your needs, you can also write your own helper functions.

| Helper Function | Description | Syntax |
|-|-|-|
| If | Checks a conditional and then follows a positive or negative path based on its value | **if** ***conditional*** |
| eq | Equals at least one of the values | **eq** ***x a b …*** |
| ne | Not equal to any value | **ne** ***x a b …*** |
| lt | Less than | **lt** ***a b*** |
| gt | Greater than | **gt** ***a b*** |
| lte | Less than or equal | **lte** ***a b*** |
| gte | Greater than or equal | **gte** ***a b*** |
| not | Not true | **not** ***x*** |
| and | Checks if all input arguments are true | **and** ***a b …*** |
| or | Check if all input arguments are false | **or** ***a b…*** |
| elementAt | Returns array element at position index | **elementAt** ***array  index*** |
| charAt | Returns char at position index | **charAt** ***string index*** |
| length | Returns array length | **length** ***array*** |
| strLength | Returns string length | **strLength** ***string*** |
| slice | Returns part of array between start and end positions (end not included) | **slice** ***array start end*** |
| strSlice | Returns part of string between start and end positions (end not included) | **strSlice** ***string start end***
| split | Splits the string based on regex. e.g. (split "a,b,c" ",") | **split** ***string regex*** |
| replace | Replaces text in a string using a regular expression | **replace** ***string searchRegex replaceStr*** |
| match | Returns an array containing matches with a regular expression | **match** ***string regex*** |
| base64Encode | Returns base64 encoded string | **base64Encode** ***string*** |
| base64Decode | Returns base64 decoded string | **base64Decode** ***string*** |
| escapeSpecialChars | Returns string with special chars escaped | **escapeSpecialChars** ***string*** |
| unescapeSpecialChars | Returns string after removing escaping of special char | **unescapeSpecialChars** ***string*** |
| assert | Fails with message if predicate is false | **assert** ***predicate message*** |
| evaluate | Returns template result object | **evaluate** ***templatePath inObj*** |
| getFieldRepeats | Returns repeat list for a field | **getFieldRepeats** ***fieldData*** |
| getFirstSegments | Returns first instance of the segments | **getFirstSegments** ***message segment1 segment2 …***
| getSegmentLists | Extract HL7 v2 segments | **getSegmentLists** ***message segment1 segment2 …*** |
| getRelatedSegmentList | Given a segment name and index, return the collection of related named segments | **getRelatedSegmentList** ***message parentSegmentName parentSegmentIndex childSegmentName*** |
| getParentSegment | Given a child segment name and overall message index, return the first matched parent segment | **getParentSegment** ***message childSegmentName childSegmentIndex parentSegmentName*** |
| hasSegments | Check if HL7 v2 message has segments | **hasSegments** ***message segment1 segment2 …*** |
| concat | Returns the concatenation of provided strings | **concat** ***aString bString cString …*** |
| generateUUID | Generates a guid based on a URL | **generateUUID** ***url***
| addHyphensSSN | Adds hyphens to an SSN without hyphens | **addHyphensSSN** ***SSN*** |
| addHyphensDate | Adds hyphens to a date without hyphens | **addHyphensDate** ***date*** |
| now | Provides current UTC time in YYYYMMDDHHmmssSSS format | **now** |
| formatAsDateTime | Converts an YYYYMMDDHHmmssSSS string, e.g. 20040629175400000 to dateTime format, e.g. 2004-06-29T17:54:00.000z | **formatAsDateTime** ***dateTimeString*** |
| toString | Converts to string | **toString** ***object*** |
| toJsonString | Converts to JSON string | **toJsonString** ***object*** |
| toLower | Converts string to lower case | **toLower** ***string*** |
| toUpper | Converts string to upper case | **toUpper** ***string*** |
| isNaN | Checks if the object is not a number using JavaScript isNaN | **isNaN** ***object*** |
