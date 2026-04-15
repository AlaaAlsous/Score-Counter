namespace Backend;

public static class IdGenerator
{
    private static readonly string[] Adjectives =
    [
        "fast","slow","wild","crazy","epic","legendary","angry","happy","sad","lucky",
        "unlucky","golden","silver","bronze","shiny","dark","light","neon","cosmic","galactic",
        "tiny","giant","massive","mini","secret","hidden","lost","ancient","future","cyber",
        "electric","spicy","sweet","salty","sour","frozen","burning","glowing","toxic","radioactive",
        "stealthy","noisy","silent","loud","sneaky","clumsy","smart","dumb","brave","fearless",
        "sleepy","awake","dreamy","chaotic","calm","stormy","windy","rainy","sunny","icy",
        "shadow","ghostly","haunted","cursed","blessed","holy","evil","wicked","friendly","hostile",
        "curious","confused","broken","glitched","pixel","retro","modern","futuristic","vintage",
        "weird","strange","random","funky","fresh","cool","hot","chill","cracked","insane",
        "mystic","arcane","divine","infernal","metal","wooden","plastic","rubber","liquid","solid"
    ];

    private static readonly string[] Nouns =
    [
        "duck","rocket","gold","lollipop","computer","cat","dog","dragon","wizard","ninja",
        "pirate","robot","alien","ghost","zombie","knight","king","queen","joker","clown",
        "banana","pizza","burger","taco","donut","cookie","cake","sandwich","noodle","coffee",
        "tea","cola","juice","milk","water","energy","monster","soda","icecream","chocolate",
        "laser","blaster","cannon","sword","shield","hammer","axe","bow","arrow","gun",
        "car","truck","bus","bike","motorcycle","train","plane","jet","ship","boat",
        "submarine","tank","spaceship","satellite","drone","rocketship","hovercraft","mech","turret","engine",
        "keyboard","mouse","screen","monitor","laptop","server","router","cable","pixel","code",
        "bug","glitch","patch","update","mod","plugin","app","game","console","controller",
        "cloud","storm","rain","snow","thunder","lightning","fire","water","earth","wind",
        "volcano","mountain","river","ocean","forest","jungle","desert","island","cave","valley",
        "planet","star","galaxy","universe","comet","asteroid","blackhole","nebula","orbit","space",
        "coin","gem","diamond","crystal","treasure","loot","chest","map","key","portal",
        "arena","battle","war","fight","duel","quest","mission","raid","boss","level",
        "npc","player","hero","villain","champion","warrior","mage","archer","healer","tank",
        "duckling","goose","penguin","panda","tiger","lion","wolf","bear","fox","rabbit",
        "snake","spider","shark","whale","dolphin","octopus","crab","lobster","frog","lizard",
        "hat","helmet","armor","boots","gloves","ring","necklace","cloak","mask","glasses",
        "chair","table","sofa","bed","lamp","door","window","floor","wall","ceiling",
        "phone","tablet","camera","speaker","headset","microphone","battery","charger","usb","disk",
        "toy","dice","card","board","token","coinflip","spinner","wheel","button","lever",
        "factory","city","village","castle","tower","dungeon","lab","office","school","market",
        "duckrocket","spacegoat","timemachine","cyberduck","megacat","ultrabot","hyperninja","lasergoose","pixelking","glitchwizard"
    ];

    public static string Generate(Random rnd)
    {
        if (Adjectives.Length == 0 || Nouns.Length < 2)
        {
            return Guid.NewGuid().ToString("N")[..12];
        }

        var adjective = Adjectives[rnd.Next(Adjectives.Length)];
        var noun1 = Nouns[rnd.Next(Nouns.Length)];
        var noun2 = Nouns[rnd.Next(Nouns.Length)];

        while (noun2 == noun1)
        {
            noun2 = Nouns[rnd.Next(Nouns.Length)];
        }

        return $"{Cap(adjective)}{Cap(noun1)}{Cap(noun2)}";
    }

    private static string Cap(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}