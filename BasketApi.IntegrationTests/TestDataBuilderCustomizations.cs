using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using BasketApi.Contracts;

namespace BasketApi.IntegrationTests
{
    class BasketItemForCreationCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<BasketItemModel>(c => c
                .Without(x => x.Quantity) //because of Range attribute, Autofixture will generate very high numbers; hence the workaround below
                .Without(x => x._embedded)
                .Without(x => x._links)
                .Do(x => x.Quantity = fixture.Create<int>()));
        }
    }
}
