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
        public static class ChildrenToText
        {
            public static ID DefinitionId => new ID("{11111111-1111-1111-1111-111111111112}");
            public static ID NewItemId => new ID("{11111111-1111-1111-1111-111111111113}");
            public static ID OldItemNoChildren => new ID("{11111111-1111-1111-1111-111111111114}");
            public static ID OldItemWithChildren => new ID("{11111111-1111-1111-1111-111111111115}");
            public static ID Child1Id => new ID("{11111111-1111-1111-1111-111111111116}");
            public static ID Child2Id => new ID("{11111111-1111-1111-1111-111111111117}");
            public static string ToFieldValue = "ToFieldValue";
        }

        public static class Shared
        {
            public static string EnglishLanguageCode = "en";

        }
    }

    public class BaseFakeDBTestFixture
    {
        #region Sample DB

        protected Db GetSampleDb()
        {
            return new Db
            {
                new DbItem("ChildrenToTextItems")
                {
                    Children =
                    {
                        new DbItem("Definition", TestingConstants.ChildrenToText.DefinitionId)
                        {
                            Fields =
                            {
                                new DbField("To What Field")
                                {
                                    { TestingConstants.Shared.EnglishLanguageCode, TestingConstants.ChildrenToText.ToFieldValue }
                                },
                                new DbField("Handler Class"),
                                new DbField("Handler Assembly")
                            }
                        },
                        new DbItem("New Item", TestingConstants.ChildrenToText.NewItemId)
                        {
                            Fields =
                            {
                                new DbField(TestingConstants.ChildrenToText.ToFieldValue) 
                            }
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
                }
            };
        }

        #endregion Sample DB
    }
}
