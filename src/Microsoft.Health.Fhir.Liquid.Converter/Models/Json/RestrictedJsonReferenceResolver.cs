// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.References;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models.Json
{
    /// <summary>
    /// A <see cref="JsonReferenceResolver"/> that refuses to resolve external (file or URL) JSON Schema
    /// <c>$ref</c> targets while still allowing in-document references (e.g. <c>#/definitions/...</c>).
    /// </summary>
    /// <remarks>
    /// JSON schema templates are supplied by template authors and are not trusted. By default NJsonSchema
    /// eagerly fetches any <c>http://</c>/<c>https://</c> or file <c>$ref</c> from the server process during
    /// parsing, which allows a template author to trigger server-side requests (SSRF) to arbitrary internal
    /// or external destinations. Overriding the file and URL resolution methods to fail closed blocks that
    /// behavior at the exact point where the fetch would occur, without affecting local references that are
    /// resolved entirely within the document.
    /// </remarks>
    public class RestrictedJsonReferenceResolver : JsonReferenceResolver
    {
        public RestrictedJsonReferenceResolver(JsonSchemaAppender schemaAppender)
            : base(schemaAppender)
        {
        }

        public override Task<IJsonReference> ResolveFileReferenceAsync(string filePath, CancellationToken cancellationToken = default)
        {
            throw new ExternalJsonReferenceException(filePath);
        }

        public override Task<IJsonReference> ResolveUrlReferenceAsync(string url, CancellationToken cancellationToken = default)
        {
            throw new ExternalJsonReferenceException(url);
        }
    }
}
