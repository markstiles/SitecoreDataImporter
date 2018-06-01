using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Sitecore.SharedSource.DataImporter.Tests
{
    public class BaseFakeDBTestFixture
    {

        #region IDs

        private ID SomeTemplateID => new ID("{aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa}");

        private ID SomePageId => new ID("{22222222-2222-2222-2222-222222222222}");

        private ID PageTemplateID => new ID("{bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb}");

        private ID PageId => new ID("{44444444-4444-4444-4444-444444444444}");

        #endregion IDs

        #region Fields

        private string englishLangCode = "en";

        private DbField TitleField => new DbField("Title")
        {
            { englishLangCode, "Page Title" }
        };

        #endregion Fields

        #region Sample DB

        private Db GetSampleDb()
        {
            return new Db
            {
                new DbTemplate("SomePage", SomeTemplateID)
                {
                    BaseIDs = new[] { PageTemplateID }
                },
                new DbItem("SomeOtherPage", SomePageId, SomeTemplateID)
                {
                    Children =
                    {
                        new DbItem("Page", PageId, PageTemplateID)
                        {
                            Fields =
                            {
                                TitleField
                            }
                        }
                    }
                }
            };
        }

        #endregion Sample DB
    }
}
