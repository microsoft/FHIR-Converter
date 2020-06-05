# Helper Functions

The open-source release includes a set of helper functions to assist with template creation. The current list of available helper functions is below. If these do not meet your needs, you can also write your own helper functions. Some of the helper functions are used by both the HL7 v2 to FHIR and C-CDA to FHIR implmentation, while others are specific to data type. 

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
| or | Checks if at least one input argument is true | **or** ***a b…*** |
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
| abs | Returns the absolute value of a number | **abs** ***a*** |
| ceil | Returns the next largest whole number or integer | **ceil** ***a*** |
| floor | Returns the largest integer less than or equal to a given number | **floor** ***a*** |
| max | Returns the largest of zero or more numbers | **max** ***a b …*** |
| min | Returns the lowest-valued number passed into it, or NaN if any parameter isn't a number and can't be converted into one | **min** ***a b …*** |
| pow | Returns the base to the exponent power, that is, base^exponent | **pow** ***x y*** |
| random | Returns a floating-point, pseudo-random number in the range 0 to less than 1 (inclusive of 0, but not 1) with approximately uniform distribution over that range — which you can then scale to your desired range | **random** |
| round | Returns the value of a number rounded to the nearest integer | **round** ***a*** |
| sign | Returns either a positive or negative +/- 1, indicating the sign of a number passed into the argument. If the number passed into is 0, it will return a +/- 0. Note that if the number is positive, an explicit (+) will not be returned | **sign** ***a*** |
| trunc | Returns the integer part of a number by removing any fractional digits | **trunc** ***a*** |
| add | Add two numbers: + number1 number 2 | **add** ***a b*** |
| subtract | Subtract second number from the first: - number 1 number 2 | **subtract** ***a b*** |
| multiply | Multiply two numbers: * number1 number2 | **multiply** ***a b*** |
| divide | Divide first number by the second number: / number1 number2 | **divide** ***a b*** |

## HL7 v2 Specific Helper Functions

| Helper Function | Description | Syntax |
|-|-|-|
| getFieldRepeats | Returns repeat list for a field | **getFieldRepeats** ***fieldData*** |
| getFirstSegments | Returns first instance of the segments | **getFirstSegments** ***message segment1 segment2 …***
| getSegmentLists | Extract HL7 v2 segments | **getSegmentLists** ***message segment1 segment2 …*** |
| getRelatedSegmentList | Given a segment name and index, return the collection of related named segments | **getRelatedSegmentList** ***message parentSegmentName parentSegmentIndex childSegmentName*** |
| getParentSegment | Given a child segment name and overall message index, return the first matched parent segment | **getParentSegment** ***message childSegmentName childSegmentIndex parentSegmentName*** |
| hasSegments | Check if HL7 v2 message has segments | **hasSegments** ***message segment1 segment2 …*** |

## C-CDA Specific Helper Functions

| Helper Function | Description | Syntax |
|-|-|-|
| sha1Hash |Returns sha1 hash (in hex) of given string | **sha1Hash** ***string***|
| contains | Returns true if a string includes another string | **contains** ***aString*** ***bString*** |  
