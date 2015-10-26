using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;


namespace GodlyLissandra
{
    internal class Program
    {
        /// <summary>
        ///     Lissandra's Name
        /// </summary>
        public const string ChampionName = "Lissandra";

        /// <summary>
        ///     Ice Shard
        /// </summary>
        public static Spell.Skillshot Q;

        /// <summary>
        ///     Ring of Frost
        /// </summary>
        public static Spell.Skillshot W;

        /// <summary>
        ///     Equinox
        /// </summary>
        public static Spell.Skillshot E;

        /// <summary>
        ///     Wish
        /// </summary>
        public static Spell.Targeted R;

        /// <summary>
        ///     Initializes the Menu
        /// </summary>
        public static Menu GodlyLissandra,
            ComboMenu,
            HarassMenu,
            LaneClearMenu,
            SelfUltMenu,
            InterruptMenu,
            GapcloserMenu,
            DrawingMenu,
            MiscMenu;

        /// <summary>
        ///     Gets the Player
        /// </summary>
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }

        /// <summary>
        ///     Runs when the Program Starts
        /// </summary>
        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;

        }


        /// <summary>
        ///     Called when Loading is Completed
        /// </summary>
        /// <param name="args">The loading arguments</param>
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                if (PlayerInstance.Hero != Champion.Lissandra)
                {
                    return;
                }


                Q = new Spell.Skillshot(SpellSlot.Q, 725, SkillShotType.Linear);
                W = new Spell.Skillshot(SpellSlot.W, 450, SkillShotType.Linear);
                E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear);
                R = new Spell.Targeted(SpellSlot.R, 550);


                GodlyLissandra = MainMenu.AddMenu("GodlyLissandra", "GodlyLissandr");

                // Combo Menu
                ComboMenu = GodlyLissandra.AddSubMenu("Combo", "Combo");
                ComboMenu.AddGroupLabel("Combo Setting");
                ComboMenu.Add("useQ", new CheckBox("Use Q"));
                ComboMenu.Add("useW", new CheckBox("Use W"));
                ComboMenu.Add("useE", new CheckBox("Use E"));
                ComboMenu.Add("useR", new CheckBox("Use R"));
                ComboMenu.AddSeparator();
                ComboMenu.AddGroupLabel("ManaManager");
                ComboMenu.Add("manaQ", new Slider("Min Mana % before Q", 25));
                ComboMenu.Add("manaE", new Slider("Min Mana % before E", 25));
                ComboMenu.Add("disableC", new CheckBox("Disable Mana Manager in Combo"));

                // Harass Menu
                HarassMenu = GodlyLissandra.AddSubMenu("Harass", "Harass");
                HarassMenu.AddGroupLabel("Harass Setting");
                HarassMenu.Add("useQ", new CheckBox("Use Q"));
                HarassMenu.Add("useE", new CheckBox("Use E"));
                HarassMenu.Add("useR", new CheckBox("Use R", false)); //default false
                HarassMenu.AddSeparator();
                HarassMenu.AddGroupLabel("ManaManager");
                HarassMenu.Add("manaQ", new Slider("Min Mana % before Q", 25));
                HarassMenu.Add("manaE", new Slider("Min Mana % before E", 25));

                //LaneClear Menu
                LaneClearMenu = GodlyLissandra.AddSubMenu("LaneClear", "LaneClear");
                LaneClearMenu.AddGroupLabel("LaneClear Setting");
                LaneClearMenu.Add("useQ", new CheckBox("Use Q"));
                LaneClearMenu.Add("useW", new CheckBox("Use W"));
                LaneClearMenu.Add("useE", new CheckBox("Use E"));
                LaneClearMenu.Add("manaQ", new Slider("Min Mana % before Q", 25));
                LaneClearMenu.Add("manaW", new Slider("Min Mana % Before W", 25));
                LaneClearMenu.Add("manaE", new Slider("Min Mana % before E", 25));


                // SelfUltMenu Menu
                SelfUltMenu = GodlyLissandra.AddSubMenu("AutoUlt", "AutoUlt");
                SelfUltMenu.AddGroupLabel("AutoUlt Setting");
                SelfUltMenu.Add("use R if close to death", new CheckBox("Use R if close to death"));
                SelfUltMenu.Add("MinHp", new Slider("Min HP % before Self R", 10));

                // Interrupt Menu
                InterruptMenu = GodlyLissandra.AddSubMenu("Interrupter", "Interrupter");
                InterruptMenu.AddGroupLabel("Interrupter Setting");
                InterruptMenu.Add("useR", new CheckBox("Use R on Interrupt"));

                // Gapcloser Menu
                GapcloserMenu = GodlyLissandra.AddSubMenu("Gapcloser", "Gapcloser");
                GapcloserMenu.AddGroupLabel("Gapcloser Setting");
                GapcloserMenu.Add("useW", new CheckBox("Use W on Gapcloser"));

                // Drawing Menu
                DrawingMenu = GodlyLissandra.AddSubMenu("Drawing", "Drawing");
                DrawingMenu.AddGroupLabel("Drawing Setting");
                DrawingMenu.Add("drawQ", new CheckBox("Draw Q Range"));
                DrawingMenu.Add("drawW", new CheckBox("Draw W Range"));
                DrawingMenu.Add("drawE", new CheckBox("Draw E Range"));
                DrawingMenu.Add("drawR", new CheckBox("Draw R Range"));

                // Misc Menu
                MiscMenu = GodlyLissandra.AddSubMenu("Misc", "Misc");
                MiscMenu.AddGroupLabel("Miscellaneous Setting");
                MiscMenu.Add("disableMAA", new CheckBox("Disable Minion AA"));
                MiscMenu.Add("disableCAA", new CheckBox("Disable Champion AA"));

                Chat.Print("GodlyLissandra: Initialized", Color.DodgerBlue);
                Game.OnTick += Game_OnTick;
                Drawing.OnDraw += Drawing_OnDraw;

            }
            catch (Exception e)
            {
                Chat.Print("GodlyLissandra: Exception occured while Initializing Addon. Error: " + e.Message);
            }
        }


        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var playerPosition = PlayerInstance.Position;

            if (DrawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Q.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, Q.Range, playerPosition);
            }
            if (DrawingMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, W.Range, playerPosition);
            }
            if (DrawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? SharpDX.Color.Green : SharpDX.Color.Red, E.Range, playerPosition);
            }
        }

        private static bool ManaManager(SpellSlot spellSlot)
        {
            if (MiscMenu["disableC"].Cast<CheckBox>().CurrentValue
                && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                return true;
            }

            var playerManaPercent = (PlayerInstance.Mana / PlayerInstance.MaxMana) * 100;

            if (spellSlot == SpellSlot.Q)
            {
                return MiscMenu["manaQ"].Cast<Slider>().CurrentValue <= playerManaPercent;
            }

            if (spellSlot == SpellSlot.W)
            {
                return MiscMenu["manaW"].Cast<Slider>().CurrentValue <= playerManaPercent;
            }

            if (spellSlot == SpellSlot.E)
            {
                return MiscMenu["manaE"].Cast<Slider>().CurrentValue <= playerManaPercent;
            }

            if (spellSlot == SpellSlot.R)
            {
                return MiscMenu["manaR"].Cast<Slider>().CurrentValue <= playerManaPercent;
            }

            return false;
        }

        /// <summary>
        ///     Does Combo
        /// </summary>
        private static void Combo()

        {
            if (ComboMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaQ"].Cast<Slider>().CurrentValue
                        && (target.IsValidTarget(Q.Range) && Q.IsReady()))
                    {
                        var pred = Q.GetPrediction(target);
                        //Prediction.Position.PredictCircularMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, PlayerInstance.ServerPosition);

                        if (pred.HitChance >= HitChance.High)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (ComboMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaW"].Cast<Slider>().CurrentValue
                        && (target.IsValidTarget(W.Range) && W.IsReady()))
                    {
                        var pred = W.GetPrediction(target);
                        //Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                        if (pred.HitChance >= HitChance.High)
                        {
                            PlayerInstance.Spellbook.CastSpell(W.Slot);
                        }
                    }
                }
            }

            if (ComboMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= ComboMenu["manaE"].Cast<Slider>().CurrentValue
                        && (target.IsValidTarget(E.Range) && E.IsReady()))
                    {
                        var pred = E.GetPrediction(target);
                        //Prediction.Position.PredictLinearMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                        if (pred.HitChance >= HitChance.High)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Does Harass
        /// </summary>
        private static void Harass()

        {
            if (HarassMenu["useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaQ"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                        {
                            var pred = Q.GetPrediction(target);
                            // Prediction.Position.PredictCircularMissile(target, Q.Range, Q.Radius, Q.CastDelay, Q.Speed, PlayerInstance.Position);

                            if (pred.HitChance == HitChance.High)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
            if (HarassMenu["useW"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaW"].Cast<Slider>().CurrentValue)
                        if (target.IsValidTarget(W.Range) && W.IsReady())
                        {
                            var pred = W.GetPrediction(target);
                            //Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                            if (pred.HitChance == HitChance.High)
                            {
                                PlayerInstance.Spellbook.CastSpell(W.Slot);
                            }
                        }
                }
            }
            if (HarassMenu["useE"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical, PlayerInstance.ServerPosition);

                if (target != null)
                {
                    if (PlayerInstance.ManaPercent >= HarassMenu["manaE"].Cast<Slider>().CurrentValue)
                    {
                        if (target.IsValidTarget(E.Range) && E.IsReady())
                        {
                            var pred = E.GetPrediction(target);
                            //Prediction.Position.PredictCircularMissile(target, E.Range, E.Radius, E.CastDelay, E.Speed, PlayerInstance.Position);

                            if (pred.HitChance == HitChance.High)
                            {
                                E.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        //need to test not sure if it's working
        //need to check Helsing and make it as a loop
        //to always be in use
        //private static void SelfUlt()
        // {
        //    if (ObjectManager.Player.HealthPercent <= 30)
        //  {
        //       R.Cast(ObjectManager.Player);
        // }
        // }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["useQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["useE"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["useW"].Cast<CheckBox>().CurrentValue;
            var useQs = LaneClearMenu["useQs"].Cast<Slider>().CurrentValue;
            var useWs = LaneClearMenu["useWs"].Cast<Slider>().CurrentValue;

            if (useQ && Q.IsReady() && ManaManager(SpellSlot.Q))
            {
                var target =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsValidTarget() && Q.IsInRange(t));

                foreach (var minion in target)
                {
                    var pred = Prediction.Position.PredictLinearMissile(
                        minion,
                        Q.Range,
                        Q.Width,
                        Q.CastDelay,
                        Q.Speed,
                        2,
                        PlayerInstance.ServerPosition);
                    if (pred.HitChance >= HitChance.High && pred.CollisionObjects.Count() >= useQs)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }

            if (useW && W.IsReady() && ManaManager(SpellSlot.W))
            {
                //use Obj_AI_Base or don't?
                //Help.
                //var target = EntityManager.MinionsAndMonsters.GetLaneMinions(
                //EntityManager.UnitTeam.Enemy,
                //PlayerInstance.ServerPosition,
                // W.Radius).ToArray();    
                // dont forgt to change the =
                Obj_AI_Minion[] target = EntityManager.MinionsAndMonsters.GetLaneMinions(
                    EntityManager.UnitTeam.Enemy,
                    PlayerInstance.ServerPosition,
                    W.Radius).ToArray();
                var pred = Prediction.Position.PredictCircularMissileAoe(
                    target,
                    W.Range,
                    W.Radius,
                    W.CastDelay,
                    W.Speed,
                    PlayerInstance.ServerPosition);

                foreach (var p in pred.Where(p => p.HitChance >= HitChance.High && p.CollisionObjects.Count() >= useWs))
                {
                    W.Cast(p.CastPosition);

                }
            }

            if (useE && E.IsReady() && ManaManager(SpellSlot.E))
            {
                var target = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t =>
                    t.IsValidTarget() && E.IsInRange(t));

                foreach (var minion in target)
                {
                    var pred = Prediction.Position.PredictLinearMissile(
                        minion,
                        E.Range,
                        E.Width,
                        E.CastDelay,
                        E.Speed,
                        2,
                        PlayerInstance.ServerPosition);

                    if (pred.HitChance >= HitChance.High && pred.CollisionObjects.Count() >= useQs)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
        }
    }
}