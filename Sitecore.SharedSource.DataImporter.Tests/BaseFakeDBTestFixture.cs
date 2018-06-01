using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Sitecore.SharedSource.DataImporter.Tests
{
    public static class TestingConstants
    {
        public static class Shared
        {
            public static string EnglishLanguageCode = "en";
            public static string ToWhatField = "To What Field";
            public static string HandlerClass = "Handler Class";
            public static string HandlerAssembly = "Handler Assembly";
            public static string FromWhatFields = "From What Fields";
        }

        public static class ChildrenToText
        {
            public static ID DefinitionId => new ID("{11111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{11111111-1111-1111-1111-111111111113}");
            public static ID OldItemNoChildren => new ID("{11111111-1111-1111-1111-111111111114}");
            public static ID OldItemWithChildren => new ID("{11111111-1111-1111-1111-111111111115}");
            public static ID Child1Id => new ID("{11111111-1111-1111-1111-111111111116}");
            public static ID Child2Id => new ID("{11111111-1111-1111-1111-111111111117}");

            public static string ToFieldName = "ToFieldName";
        }

        public static class DateToText
        {
            public static ID DefinitionId => new ID("{21111111-1111-1111-1111-111111111111}");
            public static ID NewItemId => new ID("{21111111-1111-1111-1111-111111111112}");
            public static ID OldItemId => new ID("{21111111-1111-1111-1111-111111111113}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string FromFieldValue = "20180601T174200Z";
        }

        public static class TemplateExtensions
        {
            public static ID TemplateID => new ID("{baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa}");
            public static ID InheritedTemplateID => new ID("{baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab}");
            public static ID PageID => new ID("{31111111-1111-1111-1111-111111111111}");
        }
    }

    public class BaseFakeDBTestFixture
    {
        #region Sample DB
        
        protected Db GetSampleDb()
        {
            return new Db
            {
                new DbItem("Children To Text Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ChildrenToText.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ChildrenToText.ToFieldName }
                        },
                        new DbItem("New Item", TestingConstants.ChildrenToText.NewItemId)
                        {
                            { TestingConstants.ChildrenToText.ToFieldName, "" }
                        },
                        new DbItem("No Children", TestingConstants.ChildrenToText.OldItemNoChildren),
                        new DbItem("With Children", TestingConstants.ChildrenToText.OldItemWithChildren)
                        {
                            Children =
                            {
                                new DbItem("Child 1", TestingConstants.ChildrenToText.Child1Id),
                                new DbItem("Child 2", TestingConstants.ChildrenToText.Child2Id)
                            }
                        }
                    }
                },
                new DbItem("Date To Text Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.DateToText.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.DateToText.ToFieldName },
                            { TestingConstants.Shared.FromWhatFields, TestingConstants.DateToText.FromFieldName }
                        },
                        new DbItem("New Item", TestingConstants.DateToText.NewItemId)
                        {
                            { TestingConstants.DateToText.ToFieldName, "" }
                        },
                        new DbItem("Old Item", TestingConstants.DateToText.OldItemId)
                        {
                            { TestingConstants.DateToText.FromFieldName, TestingConstants.DateToText.FromFieldValue }
                        }
                    }   
                },
                new DbItem("Item Extensions")
                {
                    Children =
                    {
                        new DbTemplate("Inherited Template", TestingConstants.TemplateExtensions.InheritedTemplateID),
                        new DbTemplate("Template", TestingConstants.TemplateExtensions.TemplateID)
                        {
                            BaseIDs = new[] { TestingConstants.TemplateExtensions.InheritedTemplateID }
                        },
                        new DbItem("Page", TestingConstants.TemplateExtensions.PageID, TestingConstants.TemplateExtensions.TemplateID)
                    }
                }
            };
        }

        #endregion Sample DB
    }
}
