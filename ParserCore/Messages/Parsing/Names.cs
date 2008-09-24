using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class for holding the names of all weaponskills to allow easy identification
    /// vs. Abilities.
    /// </summary>
    public static class Weaponskills
    {
        public static readonly List<string> NamesList = new List<string>(168)
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

    /// <summary>
    /// Class for the names of the avatars for separation from normal mobs.
    /// </summary>
    public static class Avatars
    {
        public static readonly List<string> NamesList = new List<string>(9)
        {
            "Carbuncle", "Fenrir", "Diabolos",
            "Ifrit", "Shiva", "Garuda", "Titan", "Ramuh", "Leviathan"
        };
    }

    /// <summary>
    /// Class for the names of the puppets for separation from normal mobs.
    /// </summary>
    public static class Puppets
    {
        public static readonly List<string> NamesList = new List<string>(128)
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
            "Bito-Rabito", "Cocoa", "Totomo"
        };

        /// <summary>
        /// Short list for only those names that contain characters normally
        /// indicative of mob names.
        /// </summary>
        public static readonly List<string> ShortNamesList = new List<string>(4)
        {
            "X-32", "V-1000", "Purute-Porute", "Bito-Rabito"
        };
    }

    /// <summary>
    /// Class for the names of the wyverns for separation from normal mobs.
    /// </summary>
    public static class Wyverns
    {
        public static readonly List<string> NamesList = new List<string>(119)
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
        public static readonly List<string> ShortNamesList = new List<string>(5)
        {
            "Karav-Marav", "Darug-Borug", "Koffla-Paffla", "Galja-Mogalja", "Levian-Movian"
        };
    }

    /// <summary>
    /// Class for the names of NPC fellows for separation from normal mobs.
    /// </summary>
    public static class NPCFellows
    {
        public static readonly List<string> NamesList = new List<string>(64)
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
            "Yoli Kohlpaka", "Zoldof"
        };

        /// <summary>
        /// Short list for only those names that contain characters normally
        /// indicative of mob names.
        /// </summary>
        public static readonly List<string> ShortNamesList = new List<string>(16)
        {
            "Balu-Falu", "Fhig Lahrv", "Burg-Ladarg", "Khuma Tagyawhan", "Ehgo-Ryuhgo",
            "Pimy Kettihl", "Kolui-Pelui", "Raka Maimhov", "Nokum-Akkum", "Sahyu Banjyao",
            "Savul-Kivul", "Sufhi Uchnouma", "Vinja-Kanja", "Tsuim Nhomango", "Yarga-Umiga",
            "Yoli Kohlpaka"
        };

    }
}
