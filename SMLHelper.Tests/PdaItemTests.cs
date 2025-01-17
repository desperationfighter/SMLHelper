﻿using System.Reflection;

namespace SMLHelper.Tests.AssestClassTests
{
    using NSubstitute;
    using NUnit.Framework;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Interfaces;
    using UnityEngine;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using TechData = V2.Crafting.RecipeData;
#endif

    [TestFixture]
    internal class PdaItemTests
    {
        [Test]
        public void VirtualMethods_DefaultBeahviors()
        {
            var pdaItem = new SimpleTestPdaItem();

            Assert.AreEqual(TechType.None, pdaItem.RequiredForUnlock);
            Assert.IsTrue(pdaItem.UnlockedAtStart);
            Assert.AreEqual("NotificationBlueprintUnlocked", pdaItem.DiscoverMessageResolved);
        }

        [Test]
        public void Patch_EventsInvoked()
        {
            // ARRANGE
            const TechType createdTechType = TechType.Accumulator;

            IPrefabHandler mockPrefabHandler = Substitute.For<IPrefabHandler>();
            ISpriteHandler mockSpriteHandler = Substitute.For<ISpriteHandler>();
            ITechTypeHandlerInternal mockTechTypeHandler = Substitute.For<ITechTypeHandlerInternal>();
            ICraftDataHandler mockCraftDataHandler = Substitute.For<ICraftDataHandler>();
            IKnownTechHandler mockKnownTechHandler = Substitute.For<IKnownTechHandler>();

            mockTechTypeHandler
                .AddTechType(Arg.Any<Assembly>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(createdTechType);

            var techData = new TechData();

            var pdaItem = new SimpleTestPdaItem
            {
                PrefabHandler = mockPrefabHandler,
                SpriteHandler = mockSpriteHandler,
                TechTypeHandler = mockTechTypeHandler,
                CraftDataHandler = mockCraftDataHandler,
                KnownTechHandler = mockKnownTechHandler,
                TechDataToReturn = techData,
            };

            // ACT
            pdaItem.Patch();

            // ASSERT
            mockCraftDataHandler.Received(1).SetTechData(createdTechType, techData);
            mockCraftDataHandler.Received(1).AddToGroup(TechGroup.Constructor, TechCategory.Constructor, createdTechType);
            mockKnownTechHandler.DidNotReceiveWithAnyArgs();
            mockTechTypeHandler.Received(1).AddTechType(Arg.Any<Assembly>(), "classId", "friendlyName", "description", true);
        }

        private class SimpleTestPdaItem : PdaItem
        {            
            public TechData TechDataToReturn { get; set; }
            public GameObject GameObjectToReturn { get; set; }

            public SimpleTestPdaItem()
                : base("classId", "friendlyName", "description")
            {
            }

            public SimpleTestPdaItem(string classId, string friendlyName, string description)
                : base(classId, friendlyName, description)
            {
            }

            public override TechGroup GroupForPDA { get; } = TechGroup.Constructor;
            public override TechCategory CategoryForPDA { get; } = TechCategory.Constructor;

            public override GameObject GetGameObject()
            {
                return this.GameObjectToReturn;
            }

            protected override TechData GetBlueprintRecipe()
            {
                return this.TechDataToReturn;
            }

            protected override Sprite GetItemSprite()
            {
                return null;
            }
        }
    }
}
