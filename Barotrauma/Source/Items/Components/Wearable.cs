﻿using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma.Items.Components
{
    class WearableSprite
    {
        public Sprite Sprite;
        public bool HideLimb;
        public LimbType DepthLimb;

        public Wearable WearableComponent;

        public WearableSprite(Wearable item, Sprite sprite, bool hideLimb, LimbType depthLimb = LimbType.None)
        {
            WearableComponent = item;
            Sprite = sprite;
            HideLimb = hideLimb;

            DepthLimb = depthLimb;
        }
    }

    class Wearable : Pickable
    {
        WearableSprite[] wearableSprites;
        LimbType[] limbType;
        Limb[] limb;

        private float armorValue;

        private Vector2 armorSector;

        [HasDefaultValue(0.0f, false)]
        public float ArmorValue
        {
            get { return armorValue; }
            set { armorValue = MathHelper.Clamp(value, 0.0f, 100.0f); }
        }

        [HasDefaultValue("0.0,360.0", false)]
        public string ArmorSector
        {
            get { return ToolBox.Vector2ToString(armorSector); }
            set 
            { 
                armorSector = ToolBox.ParseToVector2(value);
                armorSector.X = MathHelper.ToRadians(armorSector.X);
                armorSector.Y = MathHelper.ToRadians(armorSector.Y);
            }
        }

        public Vector2 ArmorSectorLimits
        {
            get { return armorSector; }
        }

        public Wearable (Item item, XElement element)
            : base(item, element)
        {
            this.item = item;

            var sprites = element.Elements().Where(x => x.Name.ToString() == "sprite").ToList();
            int spriteCount = sprites.Count;
            wearableSprites = new WearableSprite[spriteCount];
            limbType    = new LimbType[spriteCount];
            limb        = new Limb[spriteCount];

            int i = 0;
            foreach (XElement subElement in sprites)
            {
                //Rectangle sourceRect = new Rectangle(
                //    ToolBox.GetAttributeInt(subElement, "sourcex", 1),
                //    ToolBox.GetAttributeInt(subElement, "sourcey", 1),
                //    ToolBox.GetAttributeInt(subElement, "sourcewidth", 1),
                //    ToolBox.GetAttributeInt(subElement, "sourceheight", 1));

                if (subElement.Attribute("texture") == null)
                {
                    DebugConsole.ThrowError("Item \"" + item.Name + "\" doesn't have a texture specified!");
                    return;
                }

                string spritePath = subElement.Attribute("texture").Value;
                spritePath = Path.GetDirectoryName( item.Prefab.ConfigFile)+"/"+spritePath;

                var sprite = new Sprite(subElement, "", spritePath);
                wearableSprites[i] = new WearableSprite(this, sprite, ToolBox.GetAttributeBool(subElement, "hidelimb", false),
                    (LimbType)Enum.Parse(typeof(LimbType),
                    ToolBox.GetAttributeString(subElement, "depthlimb", "None"), true));

                limbType[i] = (LimbType)Enum.Parse(typeof(LimbType),
                    ToolBox.GetAttributeString(subElement, "limb", "Head"), true);                

                i++;
            }
        }
        
        public override void Equip(Character character)
        {
            picker = character;
            for (int i = 0; i < wearableSprites.Length; i++ )
            {
                Limb equipLimb  = character.AnimController.GetLimb(limbType[i]);
                if (equipLimb == null) continue;

                //something is already on the limb -> unequip it
                //if (equipLimb.WearingItem != null && equipLimb.WearingItem != this)
                //{
                //    equipLimb.WearingItem.Unequip(character);
                //}

                //sprite[i].Depth = equipLimb.sprite.Depth - 0.001f;

                item.body.Enabled = false;

                IsActive = true;

                limb[i] = equipLimb;
                equipLimb.WearingItems.Add(wearableSprites[i]);
            }
        }

        public override void Drop(Character dropper)
        {
            Unequip(picker);

            base.Drop(dropper);

            picker = null;
            IsActive = false;
        }

        public override void Unequip(Character character)
        {
            if (picker == null) return;
            for (int i = 0; i < wearableSprites.Length; i++)
            {
                Limb equipLimb = character.AnimController.GetLimb(limbType[i]);
                if (equipLimb == null) continue;

                //foreach (WearableSprite wearable in equipLimb.WearingItems)
                //{
                //    if (wearable != wearableSprites[i]) continue;

                //    equipLimb.WearingItems.Remove(wearableSprites[i]);
                //}

                equipLimb.WearingItems.RemoveAll(w=> w!=null && w==wearableSprites[i]);

                //if (equipLimb.WearingItem != this) continue;
                
                limb[i] = null;
                //equipLimb.WearingItem = null;
                //equipLimb.WearingItemSprite = null;
            }

            IsActive = false;
        }

        public override void UpdateBroken(float deltaTime, Camera cam)
        {
            Update(deltaTime, cam);
        }

        public override void Update(float deltaTime, Camera cam)
        {
            item.SetTransform(picker.SimPosition, 0.0f);
            item.SetContainedItemPositions();
            
            ApplyStatusEffects(ActionType.OnWearing, deltaTime, picker);

            PlaySound(ActionType.OnWearing, picker.WorldPosition);
        }

        protected override void RemoveComponentSpecific()
        {
            base.RemoveComponentSpecific();

            foreach (WearableSprite wearableSprite in wearableSprites)
            {
                if (wearableSprite != null && wearableSprite.Sprite != null) wearableSprite.Sprite.Remove();
            }
        }

    }
}
