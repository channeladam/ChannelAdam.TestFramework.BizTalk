﻿//-----------------------------------------------------------------------
// <copyright file="BizTalkXmlMapTestValidator.cs">
//     Copyright (c) 2016 Adam Craven. All rights reserved.
// </copyright>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

namespace ChannelAdam.TestFramework.BizTalk.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using ChannelAdam.Xml;

    using Logging;
    using Microsoft.XLANGs.BaseTypes;
    using Microsoft.BizTalk.TOM;
    using System.IO;

    public static class BizTalkXmlMapTestValidator
    {
        #region Public Methods

        public static void ValidateInputXml(TransformBase map, XNode inputXml, ISimpleLogger logger)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (inputXml == null) throw new ArgumentNullException(nameof(inputXml));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Log("Validating the input XML for the BizTalk map");
            ValidateXml(inputXml.ToString(), map.SourceSchemas, BizTalkMapSchemaUtility.GetSchemaReferenceAttributes(map));
        }

        public static void ValidateOutputXml(TransformBase map, XNode outputXml, ISimpleLogger logger)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (outputXml == null) throw new ArgumentNullException(nameof(outputXml));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Log("Validating the output XML from the BizTalk map");
            ValidateXml(outputXml.ToString(), map.TargetSchemas, BizTalkMapSchemaUtility.GetSchemaReferenceAttributes(map));
        }

        public static void ValidateXml(string xmlToValidate, IEnumerable<string> schemas, IEnumerable<SchemaReferenceAttribute> schemaReferenceAttributes)
        {
            if (schemas == null) throw new ArgumentNullException(nameof(schemas));

            var schemaSet = new XmlSchemaSet();
            foreach (var schema in schemas)
            {
                schemaSet.Add(BizTalkMapSchemaUtility.LoadSchema(schema, schemaReferenceAttributes));
            }

            var xmlValidator = new XmlValidator();
            xmlValidator.ValidateXml(xmlToValidate, schemaSet);
        }

        /// <summary>
        /// Validates the given flat file contents.
        /// </summary>
        /// <param name="flatFileContents">The contents of the flat file to validate.</param>
        /// <param name="schemaTreeWithLoadedSchema">The schema tree with the schema loaded.</param>
        /// <param name="schemaClassName">The name of the schema class.</param>
        /// <returns>The flat file contents converted to XML.</returns>
        /// <exception cref="ApplicationException">When there is a validation error.</exception>
        public static string ValidateFlatFileContents(string flatFileContents, CXSDSchemaTree schemaTreeWithLoadedSchema, string schemaClassName)
        {
            var tempFlatFileFilename = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFlatFileFilename, flatFileContents);

                return ValidateFlatFile(tempFlatFileFilename, schemaTreeWithLoadedSchema, schemaClassName);
            }
            finally
            {
                if (File.Exists(tempFlatFileFilename))
                {
                    File.Delete(tempFlatFileFilename);
                }
            }
        }

        /// <summary>
        /// Validates the given flat file.
        /// </summary>
        /// <param name="flatFileFilename">The name of the flat file to validate.</param>
        /// <param name="schemaTreeWithLoadedSchema">The schema tree with the schema loaded.</param>
        /// <param name="schemaClassName">The name of the schema class.</param>
        /// <returns>The flat file contents converted to XML.</returns>
        /// <exception cref="ApplicationException">When there is a validation error.</exception>
        public static string ValidateFlatFile(string flatFileFilename, CXSDSchemaTree schemaTreeWithLoadedSchema, string schemaClassName)
        {
            if (schemaTreeWithLoadedSchema == null) throw new ArgumentNullException(nameof(schemaTreeWithLoadedSchema));

            ITOMErrorInfo[] validationErrors = null;
            string xmlOutput = null;

            if (!schemaTreeWithLoadedSchema.ValidateInstance(flatFileFilename, Microsoft.BizTalk.TOM.OutputInstanceType.Native, schemaClassName, out validationErrors, out xmlOutput))
            {
                string message = "No details provided.";

                if (validationErrors != null)
                {
                    var messages = validationErrors.Select(e => $"Line:{e.LineNumber} Position:{e.LinePosition} {(e.IsWarning ? "Warning: " : "Error: ")} {e.ErrorInfo}");
                    message = string.Join(". " + Environment.NewLine, messages);
                }

                throw new InvalidDataException($"An error occurred while parsing/validating the contents of the flat file, or converting it to XML: {Environment.NewLine}{message}");
            }

            return xmlOutput;
        }

        #endregion
    }
}