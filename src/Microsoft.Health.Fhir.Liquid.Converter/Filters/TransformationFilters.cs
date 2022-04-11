// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Health.Anonymizer.Common;
using Microsoft.Health.Anonymizer.Common.Exceptions;
using Microsoft.Health.Anonymizer.Common.Models;
using Microsoft.Health.Anonymizer.Common.Settings;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    /// <summary>
    /// Filters for conversion
    /// </summary>
    public partial class Filters
    {
        public static object Test(object input)
        {
            return input;
        }

        public static string Redact(
            string input,
            string valueType = "others",
            bool enablePartialDates = false,
            bool enablePartialAges = false,
            bool enablePartialZipCodes = false,
            List<object> restrictedZipCodeTabulationAreas = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var restrictedZipCodes = new List<string>();
            if (restrictedZipCodeTabulationAreas != null)
            {
                restrictedZipCodes = restrictedZipCodeTabulationAreas.Select(r => r.ToString()).ToList();
            }

            var redactionSetting = new RedactSetting
            {
                EnablePartialDatesForRedact = enablePartialDates,
                EnablePartialAgesForRedact = enablePartialAges,
                EnablePartialZipCodesForRedact = enablePartialZipCodes,
                RestrictedZipCodeTabulationAreas = restrictedZipCodes,
            };

            var redactionFunction = new RedactFunction(redactionSetting);

            if (Enum.TryParse(valueType, true, out AnonymizerValueTypes anonymizerValueTypes))
            {
                return redactionFunction.Redact(input, anonymizerValueTypes);
            }

            return null;
        }

        public static string DateShift(
            string input,
            string valueType = "date",
            uint dateShiftRange = 50,
            string dateShiftKey = "",
            string dateShiftKeyPrefix = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var dateShiftSetting = new DateShiftSetting
            {
                DateShiftRange = dateShiftRange,
                DateShiftKey = dateShiftKey,
                DateShiftKeyPrefix = dateShiftKeyPrefix,
            };

            var dateShiftFunction = new DateShiftFunction(dateShiftSetting);

            if (Enum.TryParse(valueType, true, out AnonymizerValueTypes anonymizerValueTypes))
            {
                return dateShiftFunction.Shift(input, anonymizerValueTypes);
            }

            throw new AnonymizerException(AnonymizerErrorCode.DateShiftFailed, "Unsupported value type. DateShift is only applicable to Date or DateTime values.");
        }

        public static string CryptoHash(
            string input,
            string cryptoHashKey = "",
            string cryptoHashType = "Sha256")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(cryptoHashType, true, out HashAlgorithmType type))
            {
                throw new AnonymizerException(AnonymizerErrorCode.CryptoHashFailed, "Hash function not supported.");
            }

            var cryptoHashSetting = new CryptoHashSetting
            {
                CryptoHashKey = cryptoHashKey,
                CryptoHashType = type,
            };

            byte[] byteKey = cryptoHashSetting.GetCryptoHashByteKey();
            HMAC hmac = cryptoHashSetting.CryptoHashType switch
            {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
                HashAlgorithmType.Md5 => new HMACMD5(byteKey),
#pragma warning restore CA5351
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
                HashAlgorithmType.Sha1 => new HMACSHA1(byteKey),
#pragma warning restore CA5350
                HashAlgorithmType.Sha256 => new HMACSHA256(byteKey),
                HashAlgorithmType.Sha512 => new HMACSHA512(byteKey),
                HashAlgorithmType.Sha384 => new HMACSHA384(byteKey),
                _ => throw new AnonymizerException(AnonymizerErrorCode.CryptoHashFailed, "Hash function not supported."),
            };

            var resultBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(resultBytes.Select(b => b.ToString("x2")));
        }

        public static string Encrypt(
            string input,
            string encryptKey)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var encryptSetting = new EncryptSetting
            {
                EncryptKey = encryptKey,
            };

            var encryptFunction = new EncryptFunction(encryptSetting);
            var resultBytes = encryptFunction.Encrypt(input, Encoding.UTF8);
            return Convert.ToBase64String(resultBytes);
        }

        public static string Perturb(
            string input,
            double span,
            string perturbRangeType,
            int roundTo)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!Enum.TryParse(perturbRangeType, true, out PerturbRangeType type))
            {
                throw new AnonymizerException(AnonymizerErrorCode.PerturbFailed, "Perturb range type not supported.");
            }

            var perturbSetting = new PerturbSetting
            {
                Span = span,
                RangeType = type,
                RoundTo = roundTo,
            };

            perturbSetting.Validate();

            var perturbFunction = new PerturbFunction(perturbSetting);
            return perturbFunction.Perturb(input);
        }
    }
}
