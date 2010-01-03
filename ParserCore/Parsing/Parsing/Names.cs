using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class for the names of the avatars for separation from normal mobs.
    /// </summary>
    public static class GameNames
    {
        public static readonly HashSet<string> Avatars = new HashSet<string>()
        {
            "Carbuncle", "Fenrir", "Diabolos",
            "Ifrit", "Shiva", "Garuda", "Titan", "Ramuh", "Leviathan"
        };

        /// <summary>
        /// Names of the puppets for separation from normal mobs.
        /// </summary>
        public static readonly HashSet<string> Puppets = new HashSet<string>()
        {
            "Luron", "Drille", "Tournefoux", "Chafouin", "Plaisantin", "Loustic", 
            "Histrion", "Bobeche", "Bougrion", "Rouleteau", "Allouette", "Serenade", 
            "Ficelette", "Tocadie", "Caprice", "Foucade", "Capillotte", "Quenotte", 
            "Pacotille", "Comedie", "Kagekiyo", "Toraoh", "Genta", "Kintoki", "Koumei", 
            "Pamama", "Lobo", "Tsukushi", "Oniwaka", "Kenbishi", "Hannya", "Mashira", 
            "Nadeshiko", "E100", "Koume", "X-32", "Poppo", "Asuka", "Sakura", "Tao", "Mao", 
            "Gadget", "Marion", "Widget", "Quirk", "Sprocket", "Cogette", "Lecter", 
            "Coppelia", "Sparky", "Clank", "Calcobrena", "Crackle", "Ricochet", "Josette", 
            "Fritz", "Skippy", "Pino", "Mandarin", "Jackstraw", "Guignol", "Moppet", 
            "Nutcracker", "Erwin", "Otto", "Gustav", "Muffin", "Xaver", "Toni", "Ina", 
            "Gerda", "Petra", "Verena", "Rosi", "Schatzi", "Warashi", "Klingel", 
            "Clochette", "Campanello", "Kaiserin", "Principessa", "Butler", "Graf", "Caro", 
            "Cara", "Mademoiselle", "Herzog", "Tramp", "V-1000", "Hikozaemon", "Nine", 
            "Acht", "Quattro", "Zero", "Dreizehn", "Seize", "Fukusuke", "Mataemon", 
            "Kansuke", "Polichinelle", "Tobisuke", "Sasuke", "Shijimi", "Chobi", "Aurelie", 
            "Magalie", "Aurore", "Caroline", "Andrea", "Machinette", "Clarine", "Armelle",
            "Reinette", "Dorlote", "Turlupin", "Klaxon", "Bambino", "Potiron", "Fustige",
            "Amidon", "Machin", "Bidulon", "Tandem", "Prestidige", "Purute-Porute",
            "Bito-Rabito", "Cocoa", "Totomo", "Centurion", "A7V", "Scipio", "Sentinel",
            "Pioneer", "Seneschal", "Ginjin", "Amagatsu", "Dolly", "Fantoccini", "Joe",
            "Kikizaru", "Whippet", "Punchinello", "Charlie", "Midge", "Petrouchka",
            "Schneider", "Ushabti", "Noel", "Yajirobe", "Hina", "Nora", "Shoki", "Kobina",
            "Kokeshi", "Mame", "Bishop", "Marvin", "Dora", "Data", "Robin", "Robby",
            "Porlo-Moperlo", "Paroko-Puronko", "Pipima", "Gagaja", "Mobil", "Donzel",
            "Archer", "Shooter", "Stephen", "Mk.IV", "Conjurer", "Footman", "Tokotoko",
            "Sancho", "Sarumaro", "Picket", "Mushroom",
        };

        /// <summary>
        /// Short list for only those names that contain characters normally
        /// indicative of mob names.
        /// </summary>
        public static readonly HashSet<string> SpecialPuppets = new HashSet<string>()
        {
            "X-32", "V-1000", "Purute-Porute", "Bito-Rabito", "Mk.IV"
        };

        /// <summary>
        /// Names of the wyverns for separation from normal mobs.
        /// </summary>
        public static readonly HashSet<string> Wyverns = new HashSet<string>()
        {
            "Azure", "Cerulean", "Rygor", "Firewing", "Delphyne", "Ember", "Rover", 
            "Max", "Buster", "Duke", "Oscar", "Maggie", "Jessie", "Lady", "Hien", 
            "Raiden", "Lumiere", "Eisenzahn", "Pfeil", "Wuffi", "George", "Donryu", 
            "Qiqiru", "Karav-Marav", "Oboro", "Darug-Borug", "Mikan", "Vhiki", 
            "Sasavi", "Tatang", "Nanaja", "Khocha ", "Nanaja", "Khocha", "Dino", 
            "Chomper", "Huffy", "Pouncer", "Fido", "Lucy", "Jake", "Rocky", "Rex", 
            "Rusty", "Himmelskralle", "Gizmo", "Spike", "Sylvester", "Milo", "Tom", 
            "Toby", "Felix", "Komet", "Bo", "Molly", "Unryu", "Daisy", "Baron", 
            "Ginger", "Muffin", "Lumineux", "Quatrevents", "Toryu", "Tataba", 
            "Etoilazuree", "Grisnuage", "Belorage", "Centonnerre", "Nouvellune", 
            "Missy", "Amedeo", "Tranchevent", "Soufflefeu", "Etoile", "Tonnerre", 
            "Nuage", "Foudre", "Hyuh", "Orage", "Lune", "Astre", "Waffenzahn", 
            "Soleil", "Courageux", "Koffla-Paffla", "Venteuse", "Lunaire", "Tora", 
            "Celeste", "Galja-Mogalja", "Gaboh", "Vhyun", "Orageuse", "Stellaire", 
            "Solaire", "Wirbelwind", "Blutkralle", "Bogen", "Junker", "Flink", 
            "Knirps", "Bodo", "Soryu", "Wanaro", "Totona", "Levian-Movian", "Kagero", 
            "Joseph", "Paparaz", "Coco", "Ringo", "Nonomi", "Teter", "Gigima", 
            "Gododavi", "Rurumo", "Tupah", "Jyubih", "Majha",
        };

        /// <summary>
        /// Short list for only those names that contain characters normally
        /// indicative of mob names.
        /// </summary>
        public static readonly HashSet<string> SpecialWyverns = new HashSet<string>()
        {
            "Karav-Marav", "Darug-Borug", "Koffla-Paffla", "Galja-Mogalja", "Levian-Movian"
        };

        /// <summary>
        /// Names of NPC fellows for separation from normal mobs.
        /// </summary>
        public static readonly HashSet<string> NPCFellows = new HashSet<string>()
        {
            "Feliz", "Amerita", "Chanandit", "Armittie", "Balu-Falu", "Cupapa", "Fhig Lahrv",
            "Durib", "Ferdinand", "Beatrice", "Deulmaeux", "Cadepure", "Burg-Ladarg",
            "Jajuju", "Khuma Tagyawhan", "Dzapiwa", "Gunnar", "Henrietta", "Demresinaux",
            "Clearite", "Ehgo-Ryuhgo", "Kalokoko", "Pimy Kettihl", "Jugowa", "Massimo",
            "Jesimae", "Ephealgaux", "Epilleve", "Kolui-Pelui", "Mahoyaya", "Raka Maimhov",
            "Mugido", "Oldrich", "Karyn", "Gauldeval", "Liabelle", "Nokum-Akkum", "Pakurara",
            "Sahyu Banjyao", "Voldai", "Siegward", "Nanako", "Grauffemart", "Nauthima",
            "Savul-Kivul", "Ripokeke", "Sufhi Uchnouma", "Wagwei", "Theobald", "Sharlene",
            "Migaifongut", "Radille", "Vinja-Kanja", "Yawawa", "Tsuim Nhomango", "Zayag",
            "Zenji", "Sieghilde", "Romidiant", "Vimechue", "Yarga-Umiga", "Yufafa",
            "Yoli Kohlpaka", "Zoldof",
            // Names with spaces are shortened to first name only when in combat:
            "Fhig", "Khuma", "Pimy", "Raka", "Sahyu", "Sufhi", "Tsuim", "Yoli"
        };

        /// <summary>
        /// Short list for only those names that contain characters normally
        /// indicative of mob names.
        /// </summary>
        public static readonly HashSet<string> SpecialNPCFellows = new HashSet<string>()
        {
            "Balu-Falu", "Fhig Lahrv", "Burg-Ladarg", "Khuma Tagyawhan", "Ehgo-Ryuhgo",
            "Pimy Kettihl", "Kolui-Pelui", "Raka Maimhov", "Nokum-Akkum", "Sahyu Banjyao",
            "Savul-Kivul", "Sufhi Uchnouma", "Vinja-Kanja", "Tsuim Nhomango", "Yarga-Umiga",
            "Yoli Kohlpaka"
        };
    }

    /// <summary>
    /// Class for localizable names of JAs and weaponskills.
    /// </summary>
    public static class JobAbilities
    {
        public static List<string> CharmJAs = GetCharmJAs();
        public static List<string> StealJAs = GetStealJAs();
        public static HashSet<string> EnfeebleJAs = GetEnfeebleJAs();
        public static HashSet<string> TwoHourJAs = GetTwoHourJAs();
        public static HashSet<string> CorRolls = GetCorRolls();
        public static HashSet<string> SelfUseJAs = GetSelfUseJAs();
        public static HashSet<string> Weaponskills = GetWeaponskills();

        /// <summary>
        /// Call the Reset function if/when the language used for the
        /// given parse changes.
        /// </summary>
        public static void Reset()
        {
            CharmJAs = GetCharmJAs();
            StealJAs = GetStealJAs();
            EnfeebleJAs = GetEnfeebleJAs();
            TwoHourJAs = GetTwoHourJAs();
            CorRolls = GetCorRolls();
            SelfUseJAs = GetSelfUseJAs();
            Weaponskills = GetWeaponskills();
        }

        #region Functions to fill the string sets
        private static List<string> GetCharmJAs()
        {
            return new List<string>()
                {
                    Resources.ParsedStrings.Charm
                };
        }

        private static List<string> GetStealJAs()
        {
            return new List<string>()
                {
                    Resources.ParsedStrings.Steal,
                    Resources.ParsedStrings.Mug
                };
        }

        private static HashSet<string> GetEnfeebleJAs()
        {
            return new HashSet<string>()
                {
                    Resources.ParsedStrings.BoxStep,
                    Resources.ParsedStrings.Quickstep,
                    Resources.ParsedStrings.ViolentFlourish,
                    Resources.ParsedStrings.DesperateFlourish,
                    Resources.ParsedStrings.LightShot,
                    Resources.ParsedStrings.DarkShot
                };
        }

        private static HashSet<string> GetTwoHourJAs()
        {
            return new HashSet<string>()
                {
                    Resources.ParsedStrings.SoulVoice,
                    Resources.ParsedStrings.Familiar,
                    Resources.ParsedStrings.Manafont,
                    Resources.ParsedStrings.AzureLore,
                    Resources.ParsedStrings.WildCard,
                    Resources.ParsedStrings.Trance,
                    Resources.ParsedStrings.BloodWeapon,
                    Resources.ParsedStrings.SpiritSurge,
                    Resources.ParsedStrings.HundredFists,
                    Resources.ParsedStrings.MijinGakure,
                    Resources.ParsedStrings.Invincible,
                    Resources.ParsedStrings.Overdrive,
                    Resources.ParsedStrings.EagleEyeShot,
                    Resources.ParsedStrings.Chainspell,
                    Resources.ParsedStrings.MeikyoShisui,
                    Resources.ParsedStrings.TabulaRasa,
                    Resources.ParsedStrings.AstralFlow,
                    Resources.ParsedStrings.PerfectDodge,
                    Resources.ParsedStrings.MightyStrikes,
                    Resources.ParsedStrings.Benediction
                };
        }

        private static HashSet<string> GetCorRolls()
        {
            return new HashSet<string>()
                {
                    Resources.ParsedStrings.CorRoll,
                    Resources.ParsedStrings.NinRoll,
                    Resources.ParsedStrings.RngRoll,
                    Resources.ParsedStrings.DrkRoll,
                    Resources.ParsedStrings.BluRoll,
                    Resources.ParsedStrings.WhmRoll,
                    Resources.ParsedStrings.PupRoll,
                    Resources.ParsedStrings.BrdRoll,
                    Resources.ParsedStrings.MnkRoll,
                    Resources.ParsedStrings.BstRoll,
                    Resources.ParsedStrings.SamRoll,
                    Resources.ParsedStrings.SmnRoll,
                    Resources.ParsedStrings.ThfRoll,
                    Resources.ParsedStrings.RdmRoll,
                    Resources.ParsedStrings.WarRoll,
                    Resources.ParsedStrings.DrgRoll,
                    Resources.ParsedStrings.PldRoll,
                    Resources.ParsedStrings.BlmRoll,
                    Resources.ParsedStrings.DncRoll,
                    Resources.ParsedStrings.SchRoll
                };
        }

        /// <summary>
        /// Self-Use JAs are JAs that never have any additional information about
        /// their effect after use.  EG: Paladinguy uses Sentinel.
        /// These JA uses can never have additional chat text lines attached to them.
        /// Add to this list as JAs are verified.
        /// </summary>
        private static HashSet<string> GetSelfUseJAs()
        {
            return new HashSet<string>()
                {
                    Resources.ParsedStrings.DivineSeal,
                    Resources.ParsedStrings.ElementalSeal,
                    Resources.ParsedStrings.Counterstance,
                    Resources.ParsedStrings.Footwork,
                    Resources.ParsedStrings.FormlessStrikes,
                    Resources.ParsedStrings.AfflatusMisery,
                    Resources.ParsedStrings.AfflatusSolace,
                    Resources.ParsedStrings.Convert,
                    Resources.ParsedStrings.Composure,
                    Resources.ParsedStrings.SneakAttack,
                    Resources.ParsedStrings.TrickAttack,
                    Resources.ParsedStrings.Feint,
                    Resources.ParsedStrings.Sentinel,
                    Resources.ParsedStrings.Fealty,
                    Resources.ParsedStrings.Souleater,
                    Resources.ParsedStrings.Pianissimo,
                    Resources.ParsedStrings.Camouflage,
                    Resources.ParsedStrings.Barrage,
                    Resources.ParsedStrings.Hasso,
                    Resources.ParsedStrings.Seigan,
                    Resources.ParsedStrings.ThirdEye,
                    Resources.ParsedStrings.Meditate,
                    Resources.ParsedStrings.Sekkanoki,
                    Resources.ParsedStrings.Yonin,
                    Resources.ParsedStrings.Innin,
                    Resources.ParsedStrings.BurstAffinity,
                    Resources.ParsedStrings.ChainAffinity,
                    Resources.ParsedStrings.SnakeEye,
                    Resources.ParsedStrings.Fold,
                    Resources.ParsedStrings.LightArts,
                    Resources.ParsedStrings.DarkArts,
                    Resources.ParsedStrings.Sublimation,
                    Resources.ParsedStrings.Penury,
                    Resources.ParsedStrings.Celerity,
                    Resources.ParsedStrings.Accession,
                    Resources.ParsedStrings.Rapture,
                    Resources.ParsedStrings.Altruism,
                    Resources.ParsedStrings.Tranquility,
                    Resources.ParsedStrings.Parsimony,
                    Resources.ParsedStrings.Alacrity,
                    Resources.ParsedStrings.Manifestation,
                    Resources.ParsedStrings.Ebullience,
                    Resources.ParsedStrings.Focalization,
                    Resources.ParsedStrings.Equanimity
                };
        }

        /// <summary>
        /// Set for holding the names of all weaponskills to allow easy identification
        /// vs. Abilities.
        /// TODO: put these in the Resources file.
        /// </summary>
        private static HashSet<string> GetWeaponskills()
        {
            return new HashSet<string>()
                {
                    // Archery
                    "Flaming Arrow", "Piercing Arrow", "Dulling Arrow", "Sidewinder", "Blast Arrow",
                    "Arching Arrow", "Empyreal Arrow", "Namas Arrow", "Trueflight",
                    // Axes
                    "Raging Axe", "Smash Axe", "Gale Axe", "Avalanche Axe", "Spinning Axe", "Rampage",
                    "Calamity", "Mistral Axe", "Decimation", "Onslaught", "Primal Rend",
                    // Clubs
                    "Shining Strike", "Seraph Strike", "Brainshaker", "Starlight", "Moonlight", 
                    "Skullbreaker", "True Strike", "Judgment", "Hexa Strike", "Black Halo", "Randgrith",
                    "Mystic Boon",
                    // Daggers
                    "Wasp Sting", "Gust Slash", "Shadowstitch", "Viper Bite", "Cyclone", "Dancing Edge",
                    "Shark Bite", "Evisceration", "Mercy Stroke", "Mandalic Stab", "Mordant Rime",
                    "Pyrrhic Kleos",
                    // Great Axes
                    "Shield Break", "Iron Tempest", "Sturmwind", "Armor Break", "Keen Edge",
                    "Weapon Break", "Raging Rush", "Full Break", "Steel Cyclone", "Metatron Torment",
                    "King's Justice",
                    // Great Katana
                    "Tachi: Enpi", "Tachi: Hobaku", "Tachi: Goten", "Tachi: Kagero", "Tachi: Jinpu",
                    "Tachi: Koki", "Tachi: Yukikaze", "Tachi: Gekko", "Tachi: Kasha", "Tachi: Kaiten",
                    "Tachi: Rana",
                    // Great Swords
                    "Hard Slash", "Power Slash", "Frostbite", "Freezebite", "Shockwave", "Crescent Moon",
                    "Sickle Moon", "Spinning Slash", "Ground Strike", "Scourge",
                    // Hand-to-Hand
                    "Combo", "Shoulder Tackle", "One Inch Punch", "Backhand Blow", "Raging Fists",
                    "Spinning Attack", "Howling Fist", "Dragon Kick", "Asuran Fists", "Final Heaven",
                    "Ascetic's Fury", "Stringing Pummel",
                    // Katana
                    "Blade: Rin", "Blade: Retsu", "Blade: Teki", "Blade: To", "Blade: Chi", "Blade: Ei",
                    "Blade: Jin", "Blade: Ten", "Blade: Ku", "Blade: Metsu", "Blade: Kamu",
                    // Marksmanship
                    "Hot Shot", "Split Shot", "Sniper Shot", "Slug Shot", "Blast Shot", "Heavy Shot",
                    "Detonator", "Coronach", "Leaden Salute",
                    // Polearms
                    "Double Thrust", "Thunder Thrust", "Raiden Thrust", "Leg Sweep", "Penta Thrust",
                    "Vorpal Thrust", "Skewer", "Wheeling Thrust", "Impulse Drive", "Geirskogul",
                    "Drakesbane",
                    // Scythes
                    "Slice", "Dark Harvest", "Shadow of Death", "Nightmare Scythe", "Spinning Scythe",
                    "Vorpal Scythe", "Guillotine", "Cross Reaper", "Spiral Hell", "Catastrophe", "Insurgency",
                    // Staves
                    "Heavy Swing", "Rock Crusher", "Shellcrusher", "Earth Crusher", "Star Burst", "Sun Burst",
                    "Full Swing", "Spirit Taker", "Retribution", "Gate of Tartarus", "Vidohunir",
                    "Garland of Bliss", "Omniscience",
                    // Swords
                    "Fast Blade", "Flat Blade", "Burning Blade", "Red Lotus Blade", "Circle Blade",
                    "Shining Blade", "Seraph Blade", "Spirits Within", "Vorpal Blade", "Swift Blade",
                    "Savage Blade", "Knights of Round", "Death Blossom", "Atonement", "Expiacion",
                    // Automaton
                    "Slapstick", "Arcuballista", "String Clipper", "Chimera Ripper", "Knockout", "Daze",
                    "Cannibal Blade", "Armor Piercer", "Bone Crusher", "Magic Mortar"
                };

        }

        #endregion
    }
}
