using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class CreateProtocolScenario
    {
        [Fact]
        public void GivenValidProtocol_ThenItCanBeCreated()
        {
            ProtocolFactory.CheckConsistency(ProtocolFactory.CreateTest());
        }
    }
}
