using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace BuildMon.UnitTests
{
    [TestFixture]
    public class buildmon_runner
    {
        [Test]
        public void given_single_build_item_when_constructed_should_display_that_item()
        {
            var expectedId = "anId";
            var item = Mock.Of<IBuildItem>(i=>i.Id==expectedId);
            var items = new List<IBuildItem>{item};

            var mockBuildSource = new Mock<IBuildSource>();
            mockBuildSource.Setup(bs => bs.StartMonitoring(It.IsAny<Action<IEnumerable<IBuildItem>>>(), It.IsAny<Action<IEnumerable<string>>>()))
                .Returns(Task.FromResult<IEnumerable<IBuildItem>>(items));

            var mockBuildDisplay = new Mock<IBuildDisplay>();

            var subject = new BuildMonRunner(  Mock.Of<IMainWindow>(), new[] { mockBuildSource.Object }, new[] { mockBuildDisplay.Object });
            subject.Start();

            // Test construction
            mockBuildDisplay.Verify(bd => bd.SetBuildItems(items));
        }
    }
}
