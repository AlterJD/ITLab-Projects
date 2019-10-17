using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebApp.Configure.Models.Configure.Interfaces;

namespace ITLab.Projects.Database
{
    public class SequenceConfigureWork<F, S> : IConfigureWork
        where F : IConfigureWork
        where S : IConfigureWork
    {
        private readonly F first;
        private readonly S second;

        public SequenceConfigureWork(F first, S second)
        {
            this.first = first;
            this.second = second;
        }
        public async Task Configure()
        {
            await first.Configure();
            await second.Configure();
        }
    }
}
