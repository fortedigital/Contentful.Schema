using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contentful.Core.Models;
using ContentfulExt.Attributes;
using ContentfulExt.Extensions;

namespace ContentfulExt.Core
{
    public class InferedContentType
    {
        public string ContentTypeId { get; set; }
        public string Name => this.Type.Name;
        public Type Type { get; set; }
        public IReadOnlyCollection<InferedContentTypeField> Fields { get; set; }

        public string DisplayField => this.Fields
            .Where(f => f.Property.PropertyType == typeof(string) && CustomAttributeExtensions
                            .GetCustomAttributes<ContentTypeDisplayFieldAttribute>((MemberInfo) f.Property).Any())
            .Select(f => f.FieldId)
            .FirstOrDefault();

        public bool IsSameAs(ContentType contentType)
        {
            if (contentType.SystemProperties.Id != this.ContentTypeId)
                return false;

            if (contentType.Name != this.Name)
                return false;

            if (contentType.DisplayField != this.DisplayField)
                return false;

            if (contentType.Fields.Count != this.Fields.Count)
                return false;

            var matchedFields = contentType.Fields
                .GroupJoin(this.Fields, ctf => ctf.Id, ictf => ictf.FieldId,
                    (cf, icf) => new {Field = cf, InferedField = icf.SingleOrDefault()});

            foreach (var fieldMatch in matchedFields)
            {
                // Field was deleted
                if (fieldMatch.InferedField == null)
                    return false;

                if (fieldMatch.InferedField.IsSameAs(fieldMatch.Field) == false)
                    return false;
            }

            return true;
        }
    }
}