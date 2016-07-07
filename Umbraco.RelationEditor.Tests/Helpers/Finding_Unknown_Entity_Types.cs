using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.RelationEditor.Tests.Helpers
{
    [TestFixture]
    public class Finding_Entity_Type_Aliases : BaseDatabaseFactoryTest
    {
        private ServiceContext serviceContext;

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            GetUmbracoContext("http://localhost", -1, null, true);
        }

        [Test]
        public void When_Entity_Has_Alias_It_Is_Alias()
        {
            const string expectedAlias = "DocTypeAlias";

            var entityMock = new Mock<IUmbracoEntity>();

            Mock.Get(serviceContext.EntityService)
                .Setup(e => e.Get(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>(), It.IsAny<bool>()))
                .Returns(entityMock.Object);

            entityMock
                .Setup(e => e.AdditionalData)
                .Returns(new Dictionary<string, object> {{"Alias", expectedAlias}});

            var alias = EntityHelper.FindAlias(UmbracoObjectTypes.DocumentType, 1);

            Assert.AreEqual(expectedAlias, alias);
        }

        [Test]
        public void When_Entity_Doesnt_Have_Alias_It_Is_Null()
        {
            var entityMock = new Mock<IUmbracoEntity>();

            Mock.Get(serviceContext.EntityService)
                .Setup(e => e.Get(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>(), It.IsAny<bool>()))
                .Returns(entityMock.Object);

            entityMock
                .Setup(e => e.AdditionalData)
                .Returns(new Dictionary<string, object>());

            var alias = EntityHelper.FindAlias(UmbracoObjectTypes.Unknown, 1);

            Assert.IsNull(alias);
        }

        [Test]
        public void When_Entity_Doesnt_Have_Additional_Data_It_Is_Null()
        {
            var entityMock = new Mock<IUmbracoEntity>();

            Mock.Get(serviceContext.EntityService)
                .Setup(e => e.Get(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>(), It.IsAny<bool>()))
                .Returns(entityMock.Object);

            var alias = EntityHelper.FindAlias(UmbracoObjectTypes.DocumentTypeContainer, 1);

            Assert.IsNull(alias);
        }

        [Test]
        public void When_Entity_Not_Found_It_Is_Null()
        {
            Mock.Get(serviceContext.EntityService)
                .Setup(e => e.Get(It.IsAny<int>(), It.IsAny<UmbracoObjectTypes>(), It.IsAny<bool>()))
                .Returns((IUmbracoEntity)null);

            var alias = EntityHelper.FindAlias(UmbracoObjectTypes.DocumentTypeContainer, 1);

            Assert.IsNull(alias);
        }

        protected override ApplicationContext CreateApplicationContext()
        {
            serviceContext = GetMockedServiceContext();
            return new ApplicationContext(
                new DatabaseContext(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), ""),
                serviceContext,
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                );
        }

        // TODO: Remove for 7.5
        private static ServiceContext GetMockedServiceContext()
        {
            return new ServiceContext(new Mock<IContentService>().Object, new Mock<IMediaService>().Object, new Mock<IContentTypeService>().Object, new Mock<IDataTypeService>().Object, new Mock<IFileService>().Object, new Mock<ILocalizationService>().Object, new Mock<IPackagingService>().Object, new Mock<IEntityService>().Object, new Mock<IRelationService>().Object, new Mock<IMemberGroupService>().Object, new Mock<IMemberTypeService>().Object, new Mock<IMemberService>().Object, new Mock<IUserService>().Object, new Mock<ISectionService>().Object, new Mock<IApplicationTreeService>().Object, new Mock<ITagService>().Object, new Mock<INotificationService>().Object, new Mock<ILocalizedTextService>().Object, new Mock<IAuditService>().Object, new Mock<IDomainService>().Object, new Mock<ITaskService>().Object, new Mock<IMacroService>().Object, (IPublicAccessService)null, (IExternalLoginService)null, (IMigrationEntryService)null);
        }
    }
}
