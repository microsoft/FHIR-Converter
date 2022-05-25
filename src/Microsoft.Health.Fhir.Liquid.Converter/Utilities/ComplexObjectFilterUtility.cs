// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Health.Fhir.Liquid.Converter.Utilities
{
    public static class ComplexObjectFilterUtility
    {
        private static readonly char[] Delimeters = new[] { '.', '[', ']' };

        public static object[] Select(object[] input, string path, string[] values)
        {
            // Split path on . and [], [5]
            Queue<string> pathKeys = SplitObjectPath(path);

            List<object> ret = new List<object>();

            foreach (object obj in input)
            {
                // Clone the queue to scope the path at this level for each loop
                var localPath = new Queue<string>(pathKeys);
                if (ObjHasValueAtPath(obj, localPath, values))
                {
                    // This object has our value at the path so add it to our return
                    ret.Add(obj);
                }
            }

            return ret.ToArray<object>();
        }

        public static bool ObjHasValueAtPath(object input, Queue<string> path, string[] values)
        {
            // If we're at the end of the path then check the value
            if (path.Count == 0)
            {
                // Return true if value is empty and check if input is in values otherwise
                return values.Count() == 0 || values.Contains(input.ToString());
            }

            // Get our key name
            var key = path.Dequeue();

            // Check if our key is an object property or an array reference
            if (key.Contains('['))
            {
                // Our required path dictates we should have an array and if not
                // then this is not a match
                if (input is object[] inputArray)
                {
                    // If key is [] then we want to loop over every object in the array
                    // and check for our future paths
                    if (key == "[]")
                    {
                        if (path.Count == 0)
                        {
                            var test = values.Intersect(inputArray);
                            return values.Count() == 0 || values.Intersect(inputArray).Any();
                        }

                        foreach (object obj in inputArray)
                        {
                            // Clone the queue to scope the path at this level for each loop
                            var localPath = new Queue<string>(path);
                            if (ObjHasValueAtPath(obj, localPath, values))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    // If our array key references a specific index
                    else
                    {
                        // Trim and parse out our numeric index
                        var indexStr = key.TrimStart('[').TrimEnd(']');
                        var index = int.Parse(indexStr);

                        if (index >= 0 && inputArray.Count() > index)
                        {
                            // Recurse
                            return ObjHasValueAtPath(inputArray[index], path, values);
                        }
                        else
                        {
                            // It's not possible for this array to have the element specified
                            // so this is not a match
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            // Our key is referencing an object property
            else
            {
                return input is Dictionary<string, object> inputObject &&
                    inputObject.ContainsKey(key) &&
                    ObjHasValueAtPath(inputObject[key], path, values);
            }
        }

        // This function splits a string into an array of parts like
        // `test.path[].to[5].value` -> ['test', 'path', '[]', 'to', '[5]', 'value']
        public static Queue<string> SplitObjectPath(string path)
        {
            var parts = new Queue<string>();
            var part = new StringBuilder();
            var bracket = false;

            static void PerformSplit(Queue<string> parts, StringBuilder part)
            {
                // This protects against a begining of path string edge case.
                if (part.Length > 0)
                {
                    parts.Enqueue(part.ToString());
                    part.Clear();
                }
            }

            for (int i = 0; i < path.Length; i++)
            {
                if (Delimeters.Contains(path[i]) == false)
                {
                    // Any char that isn't a delimeter gets saved
                    part.Append(path[i]);
                }

                switch (path[i])
                {
                    case '[':
                        PerformSplit(parts, part);

                        // Brackets do not get stripped
                        part.Append(path[i]);

                        // Begin a bracket split
                        bracket = true;
                        break;

                    case ']':
                        // Brackets do not get stripped
                        part.Append(path[i]);

                        // Break out of the bracket split
                        bracket = false;
                        break;

                    case '.':
                        if (!bracket)
                        {
                            PerformSplit(parts, part);
                        }

                        break;
                }

                // Cover our end of string edgecase
                if (i == path.Length - 1)
                {
                    PerformSplit(parts, part);
                }
            }

            return parts;
        }
    }
}
