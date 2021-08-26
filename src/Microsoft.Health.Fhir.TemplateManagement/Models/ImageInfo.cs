// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.ContainerRegistry.Models;
using Microsoft.Health.Fhir.TemplateManagement.Exceptions;

namespace Microsoft.Health.Fhir.TemplateManagement.Models
{
    public class ImageInfo
    {
        private const char ImageDigestDelimiter = '@';
        private const char ImageTagDelimiter = ':';
        private const char ImageRegistryDelimiter = '/';

        // Reference docker's image name format: https://docs.docker.com/engine/reference/commandline/tag/#extended-description
        private static readonly Regex ImageNameRegex = new Regex(@"^[a-z0-9]+(([_\.]|_{2}|\-+)[a-z0-9]+)*(\/[a-z0-9]+(([_\.]|_{2}|\-+)[a-z0-9]+)*)*$");

        public ImageInfo(string registry, string imageName, string tag = "latest", string digest = null)
        {
            Registry = registry;
            ImageName = imageName;
            Tag = tag;
            Digest = digest;
            if (string.IsNullOrEmpty(tag))
            {
                Tag = "latest";
            }

            ImageReference = string.Format(Constants.ImageReferenceFormat, Registry, ImageName, Label);
        }

        public string ImageName { get; set; }

        public string Tag { get; set; } = "latest";

        public string Digest { get; set; } = null;

        public string Registry { get; set; }

        public string Label
        {
            get { return Digest ?? Tag; }
        }

        public string ImageReference { get; set; }

        public bool IsDefaultTemplate()
        {
            return IsDefaultTemplateImageReference(ImageReference);
        }

        public static bool IsDefaultTemplateImageReference(string imageReference)
        {
            return DefaultTemplateInfo.DefaultTemplateMap.ContainsKey(imageReference);
        }

        public static bool IsValidImageReference(string imageReference)
        {
            try
            {
                ValidateImageReference(imageReference);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ValidateImageReference(string imageReference)
        {
            if (IsDefaultTemplateImageReference(imageReference))
            {
                return;
            }

            var registryDelimiterPosition = imageReference.IndexOf(ImageRegistryDelimiter, StringComparison.InvariantCultureIgnoreCase);
            if (registryDelimiterPosition <= 0 || registryDelimiterPosition == imageReference.Length - 1)
            {
                throw new ImageReferenceException(TemplateManagementErrorCode.InvalidReference, "Template reference format is invalid: registry server is missing.");
            }

            imageReference = imageReference[(registryDelimiterPosition + 1) ..];
            string imageName = imageReference;
            if (imageReference.Contains(ImageDigestDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = ParseImageMeta(imageReference, ImageDigestDelimiter);
                if (string.IsNullOrEmpty(imageMeta.Item1) || string.IsNullOrEmpty(imageMeta.Item2))
                {
                    throw new ImageReferenceException(TemplateManagementErrorCode.InvalidReference, "Template reference format is invalid: digest is missing.");
                }

                imageName = imageMeta.Item1;
            }
            else if (imageReference.Contains(ImageTagDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = ParseImageMeta(imageReference, ImageTagDelimiter);
                if (string.IsNullOrEmpty(imageMeta.Item1) || string.IsNullOrEmpty(imageMeta.Item2))
                {
                    throw new ImageReferenceException(TemplateManagementErrorCode.InvalidReference, "Template reference format is invalid: tag is missing.");
                }

                imageName = imageMeta.Item1;
            }

            ValidateImageName(imageName);
        }

        public static ImageInfo CreateFromImageReference(string imageReference)
        {
            ValidateImageReference(imageReference);

            var registryDelimiterPosition = imageReference.IndexOf(ImageRegistryDelimiter, StringComparison.InvariantCultureIgnoreCase);
            var registryServer = imageReference.Substring(0, registryDelimiterPosition);
            string imageMetaStr = imageReference[(registryDelimiterPosition + 1) ..];

            if (imageMetaStr.Contains(ImageDigestDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = ParseImageMeta(imageMetaStr, ImageDigestDelimiter);
                return new ImageInfo(registryServer, imageMeta.Item1, tag: null, digest: imageMeta.Item2);
            }

            if (imageMetaStr.Contains(ImageTagDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                Tuple<string, string> imageMeta = ParseImageMeta(imageMetaStr, ImageTagDelimiter);
                return new ImageInfo(registryServer, imageMeta.Item1, tag: imageMeta.Item2);
            }

            return new ImageInfo(registryServer, imageMetaStr);
        }

        private static Tuple<string, string> ParseImageMeta(string input, char delimiter)
        {
            var index = input.IndexOf(delimiter, StringComparison.InvariantCultureIgnoreCase);
            return new Tuple<string, string>(input.Substring(0, index), input[(index + 1) ..]);
        }

        private static void ValidateImageName(string imageName)
        {
            if (!ImageNameRegex.IsMatch(imageName))
            {
                throw new ImageReferenceException(TemplateManagementErrorCode.InvalidReference, @"Image name is invalid. Image name should contains lowercase letters, digits and separators. The valid format is ^[a-z0-9]+(([_\.]|_{2}|\-+)[a-z0-9]+)*(\/[a-z0-9]+(([_\.]|_{2}|\-+)[a-z0-9]+)*)*$");
            }
        }
    }
}
