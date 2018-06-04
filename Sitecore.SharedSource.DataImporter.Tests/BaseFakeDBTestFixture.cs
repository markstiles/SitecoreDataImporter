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

        public static class FromChildValueToText
        {
            public static ID DefinitionId => new ID("{71111111-1111-1111-1111-111111111111}");
            public static ID NewItemId => new ID("{71111111-1111-1111-1111-111111111112}");
            public static ID OldItemId => new ID("{71111111-1111-1111-1111-111111111113}");
            public static ID OldItemNoChildrenId => new ID("{71111111-1111-1111-1111-111111111115}");
            public static ID ChildTemplateId => new ID("{71111111-1111-1111-1111-111111111114}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string FromFieldValue = "20180601T174200Z";
            public static string ChildTemplateFieldName = "Child Template";
        }

        public static class ListToGuid
        {
            public static ID DefinitionId => new ID("{81111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{81111111-1111-1111-1111-111111111113}");
            public static ID SourceItemId => new ID("{81111111-1111-1111-1111-111111111115}");

            public static string ToFieldName = "ToFieldName";
            public static string SourceListFieldName = "Source List";
            public static string SourceListFieldValue = "/sitecore/content/List To Guid/Source List";
            public static string SourceItemName = "ItemName";
        }

        public static class ToDate
        {
            public static ID DefinitionId => new ID("{41111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{41111111-1111-1111-1111-111111111113}");
            public static ID OldItemId => new ID("{41111111-1111-1111-1111-111111111114}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string FromFieldValue = "Some Other Value";
        }

        public static class ToNumber
        {
            public static ID DefinitionId => new ID("{61111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{61111111-1111-1111-1111-111111111113}");
            public static ID OldItemId => new ID("{61111111-1111-1111-1111-111111111114}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string ImportValue = "5";
        }

        public static class ToStaticValue
        {
            public static ID DefinitionId => new ID("{51111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{51111111-1111-1111-1111-111111111113}");
            public static ID OldItemId => new ID("{51111111-1111-1111-1111-111111111114}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string ValueFieldName = "Value";
            public static string ValueFieldValue = "ABCDEFG";
        }

        public static class ToText
        {
            public static ID DefinitionId => new ID("{31111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{31111111-1111-1111-1111-111111111113}");
            public static ID OldItemId => new ID("{31111111-1111-1111-1111-111111111114}");

            public static string ToFieldName = "ToFieldName";
            public static string FromFieldName = "FromFieldName";
            public static string FromFieldValue = "Some Other Value";
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
                new DbItem("From Child Value To Text")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.FromChildValueToText.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.FromChildValueToText.ToFieldName },
                            { TestingConstants.Shared.FromWhatFields, TestingConstants.FromChildValueToText.FromFieldName },
                            { TestingConstants.FromChildValueToText.ChildTemplateFieldName, TestingConstants.FromChildValueToText.ChildTemplateId.ToString() }
                        },
                        new DbItem("New Item", TestingConstants.FromChildValueToText.NewItemId)
                        {
                            { TestingConstants.FromChildValueToText.ToFieldName, "" }
                        },
                        new DbItem("Old Item", TestingConstants.FromChildValueToText.OldItemId)
                        {
                            Children = {
                                new DbItem("New Item Child", new ID(Guid.NewGuid()), TestingConstants.FromChildValueToText.ChildTemplateId)
                                {
                                    { TestingConstants.FromChildValueToText.FromFieldName, TestingConstants.FromChildValueToText.FromFieldValue }
                                }
                            }
                        },
                        new DbItem("Old Item No Children", TestingConstants.FromChildValueToText.OldItemNoChildrenId)
                    }
                },
                new DbItem("List To Guid")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ListToGuid.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ListToGuid.ToFieldName },
                            { TestingConstants.ListToGuid.SourceListFieldName, TestingConstants.ListToGuid.SourceListFieldValue }
                        },
                        new DbItem("New Item", TestingConstants.ListToGuid.NewItemId)
                        {
                            { TestingConstants.ListToGuid.ToFieldName, "" }
                        },
                        new DbItem("Source List")
                        {
                            Children =
                            {
                                new DbItem(TestingConstants.ListToGuid.SourceItemName, TestingConstants.ListToGuid.SourceItemId)
                            }
                        }
                    }
                },
                new DbItem("To Date Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ToDate.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ToDate.ToFieldName },
                            { TestingConstants.Shared.FromWhatFields, TestingConstants.ToDate.FromFieldName }
                        },
                        new DbItem("New Item", TestingConstants.ToDate.NewItemId)
                        {
                            { TestingConstants.ToDate.ToFieldName, "" }
                        },
                        new DbItem("Old Item", TestingConstants.ToDate.OldItemId)
                        {
                            { TestingConstants.ToDate.FromFieldName, TestingConstants.ToDate.FromFieldValue }
                        }
                    }
                },
                new DbItem("To Number Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ToNumber.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ToNumber.ToFieldName },
                        },
                        new DbItem("New Item", TestingConstants.ToNumber.NewItemId)
                        {
                            { TestingConstants.ToNumber.ToFieldName, "" }
                        }
                    }
                },
                new DbItem("To Static Value Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ToStaticValue.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ToStaticValue.ToFieldName },
                            { TestingConstants.Shared.FromWhatFields, TestingConstants.ToStaticValue.FromFieldName },
                            { TestingConstants.ToStaticValue.ValueFieldName, TestingConstants.ToStaticValue.ValueFieldValue }
                        },
                        new DbItem("New Item", TestingConstants.ToStaticValue.NewItemId)
                        {
                            { TestingConstants.ToStaticValue.ToFieldName, "" }
                        }
                    }
                },
                new DbItem("To Text Items")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ToText.DefinitionId)
                        {
                            { TestingConstants.Shared.ToWhatField, TestingConstants.ToText.ToFieldName },
                            { TestingConstants.Shared.FromWhatFields, TestingConstants.ToText.FromFieldName }
                        },
                        new DbItem("New Item", TestingConstants.ToText.NewItemId)
                        {
                            { TestingConstants.ToText.ToFieldName, "" }
                        },
                        new DbItem("Old Item", TestingConstants.ToText.OldItemId)
                        {
                            { TestingConstants.ToText.FromFieldName, TestingConstants.ToText.FromFieldValue }
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
