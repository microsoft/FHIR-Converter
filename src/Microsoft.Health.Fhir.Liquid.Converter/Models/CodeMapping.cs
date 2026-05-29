// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public class CodeMapping : Drop
    {
        public CodeMapping(Dictionary<string, Dictionary<string, Dictionary<string, string>>> mapping)
        {
            Mapping = mapping;
        }

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Mapping { get; set; }

        /// <summary>
        /// Appends mappings from another CodeMapping instance.
        /// Throws InvalidOperationException if any key path already exists with a different value.
        /// </summary>
        /// <param name="additionalMapping">The CodeMapping to append.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="mappingToAppend"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if any key path already exists with a different value.</exception>
        public void Append(CodeMapping additionalMapping)
        {
            ArgumentNullException.ThrowIfNull(additionalMapping, nameof(additionalMapping));

            foreach (var level1 in additionalMapping.Mapping)
            {
                if (!Mapping.TryGetValue(level1.Key, out var level2Dict))
                {
                    Mapping[level1.Key] = new Dictionary<string, Dictionary<string, string>>(level1.Value);
                    continue;
                }

                foreach (var level2 in level1.Value)
                {
                    if (!level2Dict.TryGetValue(level2.Key, out var level3Dict))
                    {
                        level2Dict[level2.Key] = new Dictionary<string, string>(level2.Value);
                        continue;
                    }

                    foreach (var level3 in level2.Value)
                    {
                        if (level3Dict.TryGetValue(level3.Key, out var existingValue))
                        {
                            if (!string.Equals(existingValue, level3.Value, StringComparison.Ordinal))
                            {
                                throw new InvalidOperationException(
                                    $"Conflict at path [{level1.Key}][{level2.Key}][{level3.Key}]: " +
                                    $"existing='{existingValue}', new='{level3.Value}'");
                            }
                        }
                        else
                        {
                            level3Dict[level3.Key] = level3.Value;
                        }
                    }
                }
            }
        }
    }
}
