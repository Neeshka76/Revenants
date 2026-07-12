using System;

namespace Revenants.Helpers;

public static class NameGenerator
{
    private static readonly Random Random = new();
    
    private static readonly string[] Adjectives =
    {
        "Ancient", "Arcane", "Ashen", "Astral", "Azure", "Balanced", "Black", "Blazing",
        "Blessed", "Bold", "Brave", "Bright", "Bronze", "Burning", "Calm", "Celestial",
        "Champion", "Chaos", "Cinder", "Cloud", "Cold", "Courageous", "Crimson",
        "Crystal", "Cunning", "Cursed", "Dark", "Dawn", "Deadly", "Deep", "Diamond",
        "Divine", "Dread", "Dusky", "Ebon", "Echoing", "Elder", "Electric", "Emerald",
        "Endless", "Epic", "Eternal", "Fabled", "Fallen", "Fearless", "Feral", "Fiery",
        "Fierce", "Flaming", "Flying", "Forgotten", "Forsaken", "Frozen", "Ghostly",
        "Glorious", "Golden", "Grand", "Gray", "Great", "Grim", "Hallowed", "Hidden",
        "Holy", "Honorable", "Hollow", "Howling", "Hunter", "Ice", "Immortal", "Imperial",
        "Infernal", "Infinite", "Iron", "Ivory", "Jade", "Keen", "Legendary", "Lightning",
        "Lonely", "Lost", "Lucky", "Lunar", "Majestic", "Merciless", "Midnight",
        "Mighty", "Mystic", "Noble", "Obsidian", "Ocean", "Onyx", "Phantom", "Primal",
        "Pure", "Radiant", "Rapid", "Ravenous", "Relentless", "Restless", "Royal",
        "Runic", "Sacred", "Savage", "Scarlet", "Shadow", "Shattered", "Shining",
        "Silent", "Silver", "Sky", "Solar", "Spectral", "Spirit", "Star", "Steel",
        "Storm", "Strong", "Swift", "Thunder", "Titan", "True", "Twilight", "Unbroken",
        "Valiant", "Vengeful", "Vicious", "Void", "Wandering", "White", "Wild", "Wise",
        "Wrathful", "Young", "Zealous"
    };
    
    private static readonly string[] Nouns =
    {
        "Archer", "Assassin", "Avenger", "Bandit", "Basilisk", "Bear", "Beast",
        "Blade", "Boar", "Champion", "Claw", "Conqueror", "Crow", "Crusader",
        "Defender", "Demon", "Destroyer", "Dragon", "Drake", "Eagle", "Emperor",
        "Explorer", "Falcon", "Fang", "Fox", "Ghost", "Giant", "Gladiator",
        "Golem", "Griffin", "Guardian", "Hammer", "Harbinger", "Hawk", "Hero",
        "Hunter", "Hydra", "Juggernaut", "Keeper", "King", "Knight", "Kraken",
        "Lancer", "Legend", "Leopard", "Lion", "Lord", "Mage", "Mammoth",
        "Marauder", "Mercenary", "Monk", "Nomad", "Oracle", "Owl", "Paladin",
        "Panther", "Pathfinder", "Phoenix", "Pirate", "Predator", "Prince",
        "Protector", "Ranger", "Raven", "Reaper", "Renegade", "Rider", "Rogue",
        "Samurai", "Savior", "Scout", "Sentinel", "Serpent", "Shadow", "Shield",
        "Slayer", "Sorcerer", "Soul", "Specter", "Spirit", "Squire", "Stalker",
        "Storm", "Strider", "Sword", "Templar", "Tiger", "Titan", "Tracker",
        "Traveler", "Vanguard", "Viking", "Viper", "Voyager", "Wanderer", "Warlock",
        "Warrior", "Watcher", "Witch", "Wizard", "Wolf", "Wraith", "Wyvern",
        "Yeti", "Zealot", "Zombie", "Bull", "Cobra", "Jaguar", "Lynx", "Moose",
        "Ox", "Ram", "Rhino", "Scorpion", "Shark", "Spider", "Stag", "Talon",
        "Thunder", "Tornado", "Volcano", "Whisper", "Wind", "Winter", "Flame",
        "Frost", "Stone", "Mountain", "River", "Forest", "Void", "Star", "Moon",
        "Sun", "Comet", "Meteor", "Nova", "Eclipse", "Tempest", "Blizzard"
    };
    
    public static string Generate()
    {
        string adjective = Adjectives[Random.Next(Adjectives.Length)];
        string noun = Nouns[Random.Next(Nouns.Length)];
        return adjective + " " + noun;
    }
}