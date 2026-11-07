namespace XRL;

public static class Achievement
{
	public static readonly AchievementInfo DIE = new AchievementInfo("ACH_DIE", "Welcome to Qud", "welcome.png", "Die.", Hidden: true);

	public static readonly AchievementInfo VIOLATE_PAULI = new AchievementInfo("ACH_VIOLATE_PAULI", "The Laws of Physics Are Mere Suggestions, Vol. 1", "pauli1.png", "Violate the Pauli exclusion principle.");

	public static readonly AchievementInfo EAT_BEAR = new AchievementInfo("ACH_EAT_BEAR", "Eat an Entire Bear", "bear.png", "Kill a bear and eat it. Just eat an entire bear.");

	public static readonly AchievementInfo WEAR_OWN_FACE = new AchievementInfo("ACH_WEAR_OWN_FACE", "To Thine Own Self Be True", "face.png", "Wear your own severed face on your face.");

	public static readonly AchievementInfo DRINK_LAVA = new AchievementInfo("ACH_DRINK_LAVA", "On the Rocks", "lava.png", "Drink lava.", Hidden: true);

	public static readonly AchievementInfo GET_FUNGAL_INFECTIONS = new AchievementInfo("ACH_GET_FUNGAL_INFECTIONS", "Friend to Fungi", "fungi.png", "Host three different fungal infections at once.");

	public static readonly AchievementInfo INHABIT_GOAT = new AchievementInfo("ACH_INHABIT_GOAT", "Goat Simulator", "goat.png", "Project your mind into a goat's body.");

	public static readonly AchievementInfo KILLED_BY_TWIN = new AchievementInfo("ACH_KILLED_BY_TWIN", "Your Better Half", "eviltwin.png", "Get killed by your evil twin.", Hidden: true);

	public static readonly AchievementInfo FIND_ISNER = new AchievementInfo("ACH_FIND_ISNER", "The Spirit of Vengeance", "isner.png", "Recover the Ruin of House Isner.");

	public static readonly AchievementInfo FIND_STOPSVALINN = new AchievementInfo("ACH_FIND_STOPSVALINN", "Knight Conical", "stop.png", "Recover Stopsvalinn.");

	public static readonly AchievementInfo LOVE_SIGN = new AchievementInfo("ACH_LOVE_SIGN", "Love at First Sign", "sign.png", "Fall in love with a sign.");

	public static readonly AchievementInfo INSTALL_IMPLANT = new AchievementInfo("ACH_INSTALL_IMPLANT", "You Are Becoming", "becoming.png", "Install a cybernetic implant at a becoming nook.", Hidden: true);

	public static readonly AchievementInfo INSTALL_IMPLANT_EVERY_SLOT = new AchievementInfo("ACH_INSTALL_IMPLANT_EVERY_SLOT", "You Became", "became.png", "Have a cybernetic implant installed on every implantable body part.", Hidden: true);

	public static readonly AchievementInfo KILL_PYRAMID = new AchievementInfo("ACH_KILL_PYRAMID", "Pyramid Scheme", "pyramid.png", "Kill a chrome pyramid.");

	public static readonly AchievementInfo GET_GLOTROT = new AchievementInfo("ACH_GET_GLOTROT", "Say No More", "glotrot.png", "Contract glotrot.", Hidden: true);

	public static readonly AchievementInfo GET_IRONSHANK = new AchievementInfo("ACH_GET_IRONSHANK", "Metal Pedal", "ironshank.png", "Contract ironshank.", Hidden: true);

	public static readonly AchievementInfo GET_MONOCHROME = new AchievementInfo("ACH_GET_MONOCHROME", "All the Leaves are Grey", "monochrome.png", "Contract monochrome.", Hidden: true);

	public static readonly AchievementInfo EAT_OWN_LIMB = new AchievementInfo("ACH_EAT_OWN_LIMB", "You Are What You Eat", "eat.png", "Eat one of your own limbs.", Hidden: true);

	public static readonly AchievementInfo KILLED_BY_CHUTE_CRAB = new AchievementInfo("ACH_KILLED_BY_CHUTE_CRAB", "Shoot.", "chutecrab.png", "Get killed by a chute crab.", Hidden: true);

	public static readonly AchievementInfo FORESEE_DEATH = new AchievementInfo("ACH_FORESEE_DEATH", "Dark Tidings", "tidings.png", "Foresee your own death.");

	public static readonly AchievementInfo LOVE_YOURSELF = new AchievementInfo("ACH_LOVE_YOURSELF", "Love Thyself", "thyself.png", "Fall in love with yourself.");

	public static readonly AchievementInfo LOVED_BY_FACTION = new AchievementInfo("ACH_LOVED_BY_FACTION", "A Bond Knit with Trust", "bond.png", "Become loved by a faction.");

	public static readonly AchievementInfo HATED_BY_JOPPA = new AchievementInfo("ACH_HATED_BY_JOPPA", "The Woe of Joppa", "joppa.png", "Become hated by the villagers of Joppa.");

	public static readonly AchievementInfo LOVED_BY_NEW_BEINGS = new AchievementInfo("ACH_LOVED_BY_NEW_BEINGS", "Peekaboo", "sentient.png", "Become loved by newly sentient beings.");

	public static readonly AchievementInfo CRUSHED_UNDER_SUNS = new AchievementInfo("ACH_CRUSHED_UNDER_SUNS", "Starry Demise", "suns.png", "Be crushed under the weight of a thousand suns.");

	public static readonly AchievementInfo SPEAK_ALCHEMIST = new AchievementInfo("ACH_SPEAK_ALCHEMIST", "Quiet This Metal", "alchemist.png", "Speak to the alchemist.");

	public static readonly AchievementInfo HAVE_10_MUTATIONS = new AchievementInfo("ACH_HAVE_10_MUTATIONS", "Proteus", "proteus.png", "Have 10 mutations.");

	public static readonly AchievementInfo READ_10_BOOKS = new AchievementInfo("ACH_READ_10_BOOKS", "Litteratus", "books.png", "Read 10 books.", "STAT_BOOKS_READ", 10, int.MaxValue, Hidden: true);

	public static readonly AchievementInfo READ_100_BOOKS = new AchievementInfo("ACH_READ_100_BOOKS", "So Powerful is the Charm of Words", "books2.png", "Read 100 books.", "STAT_BOOKS_READ", 100, int.MaxValue, Hidden: true);

	public static readonly AchievementInfo KILL_100_SNAPJAWS = new AchievementInfo("ACH_KILL_100_SNAPJAWS", "Jawsnapper", "snapjaw.png", "Kill 100 snapjaws.", "STAT_SNAPJAWS_KILLED", 100);

	public static readonly AchievementInfo WATER_RITUAL_100_TIMES = new AchievementInfo("ACH_WATER_RITUAL_50_TIMES", "Your Thirst Is Mine, My Water Is Yours", "water.png", "Perform the introductory water ritual 100 times.", "STAT_WATER_RITUALS_PERFORMED", 100);

	public static readonly AchievementInfo GET_MUTATION_LEVEL_15 = new AchievementInfo("ACH_GET_MUTATION_LEVEL_15", "Mutagenic Mastery", "anomaly.png", "Advance a mutation to level 15.");

	public static readonly AchievementInfo CURE_GLOTROT = new AchievementInfo("ACH_CURE_GLOTROT", "Tongue in Cheek", "glotrot2.png", "Cure glotrot via traditional methods.", Hidden: true);

	public static readonly AchievementInfo CURE_IRONSHANK = new AchievementInfo("ACH_CURE_IRONSHANK", "Footloose", "ironshank2.png", "Cure ironshank via traditional methods.", Hidden: true);

	public static readonly AchievementInfo CURE_MONOCHROME = new AchievementInfo("ACH_CURE_MONOCHROME", "Rainbow Reading", "monochrome2.png", "Cure monochrome via traditional methods.", Hidden: true);

	public static readonly AchievementInfo LEARN_ONE_SULTAN_HISTORY = new AchievementInfo("ACH_LEARN_ONE_SULTAN_HISTORY", "Biographer", "sultan.png", "Learn the complete history of a single sultan.");

	public static readonly AchievementInfo VIOLATE_WATER_RITUAL = new AchievementInfo("ACH_VIOLATE_WATER_RITUAL", "Oathbreaker", "pariah.png", "Violate the sacred covenant of the water ritual.", Hidden: true);

	public static readonly AchievementInfo ACTIVATE_TIMECUBE = new AchievementInfo("ACH_ACTIVATE_TIMECUBE", "Cubic and Wisest Human", "time.png", "Activate a time cube.");

	public static readonly AchievementInfo WIELD_PRISM = new AchievementInfo("ACH_WIELD_PRISM", "Go on. Do it.", "prism.png", "Wield the amaranthine prism.");

	public static readonly AchievementInfo WATER_RITUAL_OBOROQORU = new AchievementInfo("ACH_WATER_RITUAL_OBOROQORU", "In Contemplation of Eons", "oboroqoru.png", "Perform the water ritual with Oboroqoru, Ape God.");

	public static readonly AchievementInfo REGENERATE_LIMB = new AchievementInfo("ACH_REGENERATE_LIMB", "Synolymb", "limb.png", "Regenerate a limb.", Hidden: true);

	public static readonly AchievementInfo DIE_BY_FALLING = new AchievementInfo("ACH_DIE_BY_FALLING", "Free Falling", "falling.png", "Fall to your death.", Hidden: true);

	public static readonly AchievementInfo KILL_TRISLUDGE = new AchievementInfo("ACH_KILL_TRISLUDGE", "Three-Sludge Monte", "sludge_3.png", "Kill a trisludge.");

	public static readonly AchievementInfo KILL_PENTASLUDGE = new AchievementInfo("ACH_KILL_PENTASLUDGE", "Five-Sludge Monte", "sludge_5.png", "Kill a pentasludge.");

	public static readonly AchievementInfo KILL_DECASLUDGE = new AchievementInfo("ACH_KILL_DECASLUDGE", "Ten-Sludge Monte", "sludge_10.png", "Kill a decasludge.");

	public static readonly AchievementInfo GET_DECAPITATED = new AchievementInfo("ACH_GET_DECAPITATED", "Hole Like a Head", "head.png", "Get decapitated.", Hidden: true);

	public static readonly AchievementInfo FIND_APPRENTICE = new AchievementInfo("ACH_FIND_APPRENTICE", "What With the Disembowelment and All", "apprentice.png", "Find one of Argyve's old apprentices.");

	public static readonly AchievementInfo FIND_GLOWPAD = new AchievementInfo("ACH_FIND_GLOWPAD", "Psst.", "glowpad.png", "Find the oddly-hued glowpad.", Hidden: true);

	public static readonly AchievementInfo COMPLIMENT_QGIRL = new AchievementInfo("ACH_COMPLIMENT_QGIRL", "That Was Nice", "qgirl.png", "Compliment Q Girl.", Hidden: true);

	public static readonly AchievementInfo WIELD_CAS_POL = new AchievementInfo("ACH_WIELD_CAS_POL", "Gemini", "swords.png", "Wield Caslainard and Polluxus.");

	public static readonly AchievementInfo DONATE_ITEM_200_REP = new AchievementInfo("ACH_DONATE_ITEM_200_REP", "Donation Level: Kasaphescence", "donation.png", "Make an offering at the Sacred Well of an artifact worth at least 200 reputation.");

	public static readonly AchievementInfo LEARN_JUMP = new AchievementInfo("ACH_LEARN_JUMP", "Leap, Frog.", "frog.png", "Convince a frog to teach you how to jump.");

	public static readonly AchievementInfo SIX_ARMS = new AchievementInfo("ACH_SIX_ARMS", "Six Arms None the Richer", "six_arms.png", "Have six arms.");

	public static readonly AchievementInfo VORTICES_ENTERED = new AchievementInfo("ACH_VORTICES_ENTERED", "Cosmo-Chrononaut", "vortex.png", "Enter 25 spacetime vortices.", "STAT_VORTICES_ENTERED", 25, int.MaxValue, Hidden: true);

	public static readonly AchievementInfo RECRUIT_HIGH_PRIEST = new AchievementInfo("ACH_RECRUIT_HIGH_PRIEST", "Mechanimist Reformer", "priest.png", "Convince the High Priest of the Stilt to join your party.");

	public static readonly AchievementInfo OVERDOSE_HULKHONEY = new AchievementInfo("ACH_OVERDOSE_HULKHONEY", "Aaaaaaaaargh!", "rage.png", "Overdose on a hulk honey tonic.", Hidden: true);

	public static readonly AchievementInfo WATER_RITUAL_MAMON = new AchievementInfo("ACH_WATER_RITUAL_MAMON", "Feast Upon the Goat Hearts! *cheers*", "mamon.png", "Perform the water ritual with Mamon Souldrinker.");

	public static readonly AchievementInfo EQUIP_FLUMEFLIER = new AchievementInfo("ACH_EQUIP_FLUMEFLIER", "Rocket Bear", "flume.png", "Equip the Flume-Flier of the Sky-Bear.");

	public static readonly AchievementInfo CLONE_CTESIPHUS = new AchievementInfo("ACH_CLONE_CTESIPHUS", "Two Cats Are Better Than One", "ctesiphus.png", "Clone Ctesiphus.");

	public static readonly AchievementInfo CLONES_10 = new AchievementInfo("ACH_10_CLONES", "Me, Myself, and I", "clones.png", "Share the map with 10 clones of yourself.");

	public static readonly AchievementInfo CLONES_30 = new AchievementInfo("ACH_30_CLONES", "Clonal Colony", "clones2.png", "Share the map with 30 clones of yourself.");

	public static readonly AchievementInfo KILL_OBOROQORU = new AchievementInfo("ACH_KILL_OBOROQORU", "The Woe of Apes", "woeofapes.png", "Kill Oboroqoru, Ape God.");

	public static readonly AchievementInfo KILL_AOYG = new AchievementInfo("ACH_KILL_AOYG", "Bricks Can Be Thrown, Too", "brick.png", "Kill Aoyg-no-Longer.");

	public static readonly AchievementInfo RECOVER_KINDRISH = new AchievementInfo("ACH_RECOVER_KINDRISH", "Close the Loop", "kindrish.png", "Recover Kindrish.");

	public static readonly AchievementInfo VILLAGES_100 = new AchievementInfo("ACH_100_VILLAGES", "Tourist", "tourist.png", "Visit 100 generated villages.", "STAT_VILLAGES_VISITED", 100);

	public static readonly AchievementInfo GLIMMER_20 = new AchievementInfo("ACH_20_GLIMMER", "Sight Older Than Sight", "glimmer.png", "Discover psychic glimmer.", Hidden: true);

	public static readonly AchievementInfo GLIMMER_40 = new AchievementInfo("ACH_40_GLIMMER", "What Are Directions on a Space That Cannot Be Ordered?", "glimmer40.png", "Attain 40 psychic glimmer and discover the vast psychic aether.", Hidden: true);

	public static readonly AchievementInfo GLIMMER_100 = new AchievementInfo("ACH_100_GLIMMER", "Star-Eye Esper", "glimmer100.png", "Attain 100 psychic glimmer.", Hidden: true);

	public static readonly AchievementInfo GLIMMER_200 = new AchievementInfo("ACH_200_GLIMMER", "The Quasar Mind", "quasar.png", "Attain 200 psychic glimmer.", Hidden: true);

	public static readonly AchievementInfo ABSORB_PSYCHE = new AchievementInfo("ACH_ABSORB_PSYCHE", "There Can Be Only One", "onlyone.png", "Encode the psionic bits of someone's psyche onto the holographic boundary of your own psyche.");

	public static readonly AchievementInfo TRUE_GLIMMER = new AchievementInfo("ACH_TRUE_GLIMMER", "Glimmer of Truth", "glimmertruth.png", "Attain 20 psychic glimmer as a true kin.", Hidden: true);

	public static readonly AchievementInfo RECIPES_100 = new AchievementInfo("ACH_100_RECIPES", "Sultan of Salt", "recipes.png", "Invent 100 recipes.", "STAT_RECIPES_INVENTED", 100);

	public static readonly AchievementInfo COOKED_FLUX = new AchievementInfo("ACH_COOKED_FLUX", "Absolute Unit", "absoluteunit.png", "Successfully cook a meal with neutron flux as an ingredient.");

	public static readonly AchievementInfo COOKED_EXTRADIMENSIONAL = new AchievementInfo("ACH_COOKED_EXTRADIMENSIONAL", "Non-Locally Sourced", "nonlocal.png", "Cook a meal with an extradimensional limb as an ingredient.");

	public static readonly AchievementInfo ATE_SURPRISE = new AchievementInfo("ACH_ATE_SURPRISE", "Surprise!", "cloaca.png", "Eat the Cloaca Surprise.");

	public static readonly AchievementInfo ATE_CRYSTAL_DELIGHT = new AchievementInfo("ACH_ATE_CRYSTAL_DELIGHT", "My Many Facets", "crystaldelight.png", "Eat the Crystal Delight.");

	public static readonly AchievementInfo ATE_HUMBLE_PIE = new AchievementInfo("ACH_ATE_HUMBLE_PIE", "On Second Thought", "pie.png", "Eat a serving of humble pie.");

	public static readonly AchievementInfo CROSS_BRIGHTSHEOL = new AchievementInfo("ACH_CROSS_BRIGHTSHEOL", "From Thyn Heres Shaken the Wet and Olde Lif", "brightsheol.png", "Cross into Brightsheol.", Hidden: true);

	public static readonly AchievementInfo TRAVEL_TZIMTZLUM = new AchievementInfo("ACH_TRAVEL_TZIMTZLUM", "All Those Who Wander", "wander.png", "Travel to the pocket dimension Tzimtzlum.", Hidden: true);

	public static readonly AchievementInfo WEAR_OTHERPEARL = new AchievementInfo("ACH_WEAR_OTHERPEARL", "The Recitation of the Drowning of Eudoxia by the Witches of Moonhearth", "otherpearl.png", "Wear the Otherpearl.", Hidden: true);

	public static readonly AchievementInfo ENTER_MAK_CLAM = new AchievementInfo("ACH_ENTER_MAK_CLAM", "A Clammy Reception", "clam.png", "Enter Mak's clam in the Yd Freehold.", Hidden: true);

	public static readonly AchievementInfo COMPLETE_WATERVINE = new AchievementInfo("ACH_COMPLETE_WATERVINE", "What's Eating the Watervine?", "mehmet.png", "Complete the quest What's Eating the Watervine?");

	public static readonly AchievementInfo CLAMS_ENTERED_100 = new AchievementInfo("ACH_100_CLAMS_ENTERED", "Byevalve", "ticket.png", "Enter 100 giant clams.", "STAT_CLAMS_ENTERED", 100);

	public static readonly AchievementInfo RECAME = new AchievementInfo("ACH_RECAME", "You Recame", "recame.png", "Rematerialize in the corporeal realm.", Hidden: true);

	public static readonly AchievementInfo GAVEALL_REPULSIVE_DEVICE = new AchievementInfo("ACH_GAVEALL_REPULSIVE_DEVICE", "Dayenu", "children.png", "Give the repulsive device to all four children of the Tomb.", "STAT_GAVE_REPULSIVE_DEVICE", new string[4] { "STAT_GAVE_REPULSIVE_DEVICE_NACHAM", "STAT_GAVE_REPULSIVE_DEVICE_VAAM", "STAT_GAVE_REPULSIVE_DEVICE_DAGASHA", "STAT_GAVE_REPULSIVE_DEVICE_KAH" });

	public static readonly AchievementInfo SLYNTH_HOME = new AchievementInfo("ACH_SLYNTH_HOME", "Belong, Friends", "belong.png", "Help the slynth find a new home.");

	public static readonly AchievementInfo GIFTED_10ASTERISK = new AchievementInfo("ACH_GIFTED_10ASTERISK", "Token of Gratitude", "asterisk.png", "Be gifted a 10-pointed asterisk.");

	public static readonly AchievementInfo BEAMSPLIT_SPACE_INVERTER = new AchievementInfo("ACH_BEAMSPLIT_SPACE_INVERTER", "The Laws of Physics Are Mere Suggestions, Vol. 2", "physics2.png", "Exploit wave-particle duality to clone yourself.");

	public static readonly AchievementInfo TURNED_STONE = new AchievementInfo("ACH_TURNED_STONE", "Aetalag", "aetalag.png", "Be turned to stone.", Hidden: true);

	public static readonly AchievementInfo WINKED_OUT = new AchievementInfo("ACH_WINKED_OUT", "I-", "wink.png", "Wink out of existence.");

	public static readonly AchievementInfo SWALLOWED_WHOLE = new AchievementInfo("ACH_SWALLOWED_WHOLE", "*gulp*", "gulp.png", "Be swallowed whole.", Hidden: true);

	public static readonly AchievementInfo TRANSMUTED_GEM = new AchievementInfo("ACH_TRANSMUTED_GEM", "Jeweled Dusk", "gemstone.png", "Be transmuted into a gemstone.", Hidden: true);

	public static readonly AchievementInfo WEAR_6_FACES = new AchievementInfo("ACH_WEAR_6_FACES", "The Narrowing Sky", "face_overlap.png", "Wear each of the six Faces of the sultanate.", "STAT_WEAR_FACES", new string[6] { "STAT_WEAR_FACE_1", "STAT_WEAR_FACE_2", "STAT_WEAR_FACE_3", "STAT_WEAR_FACE_4", "STAT_WEAR_FACE_5", "STAT_WEAR_FACE_6" });

	public static readonly AchievementInfo AURORAL = new AchievementInfo("ACH_AURORAL", "Dawnglider", "auroral.png", "Become auroral.");

	public static readonly AchievementInfo CHAOS_SPIEL = new AchievementInfo("ACH_CHAOS_SPIEL", "Was It Something I Said?", "chaosspiel.png", "Invoke the Chaos Spiel.");

	public static readonly AchievementInfo MUTATION_FROM_GAMMAMOTH = new AchievementInfo("ACH_MUTATION_FROM_GAMMAMOTH", "Lottery Winner", "lottery.png", "Gain a mutation from a gamma moth's mutating gaze.");

	public static readonly AchievementInfo DRAMS_BRAIN_BRINE_20 = new AchievementInfo("ACH_20_DRAMS_BRAIN_BRINE", "The Psychal Chorus", "brain.png", "Drink 20 drams of brain brine.", "STAT_BRAIN_BRINE_DRAMS_DRUNK", 20);

	public static readonly AchievementInfo BESTOW_LIFE_20 = new AchievementInfo("ACH_BESTOW_LIFE_20", "Become as Gods", "becomeasgods.png", "Bestow life to 20 inanimate objects.", "STAT_BESTOW_LIFE", 20);

	public static readonly AchievementInfo TATTOO_SELF = new AchievementInfo("ACH_TATTOO_SELF", "Live and Ink", "tattoo.png", "Tattoo yourself.");

	public static readonly AchievementInfo DISAPPOINT_HEB = new AchievementInfo("ACH_DISAPPOINT_HEB", "tsk tsk", "disappoint.png", "Disappoint a highly entropic being.");

	public static readonly AchievementInfo RECOVER_RELIC = new AchievementInfo("ACH_RECOVER_RELIC", "Raisins in the Layer Cake", "relic.png", "Recover a relic from a historic site.");

	public static readonly AchievementInfo SIX_DAY_STILT = new AchievementInfo("ACH_SIX_DAY_STILT", "May the Ground Shake But the Six Day Stilt Never Tumble", "stilt.png", "Travel to the Six Day Stilt.");

	public static readonly AchievementInfo STONED_RED_ROCK = new AchievementInfo("ACH_STONED_RED_ROCK", "Red Rock Hazing Ritual", "red_rock.png", "Be stoned to death by a baboon on the surface of Red Rock.", Hidden: true);

	public static readonly AchievementInfo LEARN_SECRET_FROM_MUMBLEMOUTH = new AchievementInfo("ACH_LEARN_SECRET_FROM_MUMBLEMOUTH", "Mumblecore", "mumble.png", "Learn a secret from the microbiome.");

	public static readonly AchievementInfo HEAD_EXPLODE = new AchievementInfo("ACH_HEAD_EXPLODE", "Open Your Mind", "skull2.png", "Have your head explode.");

	public static readonly AchievementInfo WEIRDWIRE_CONDUIT = new AchievementInfo("ACH_WEIRDWIRE_CONDUIT", "Weirdwire Conduit... Eureka!", "weirdwire.png", "Help Argyve build the contraption.");

	public static readonly AchievementInfo WILLING_SPIRIT = new AchievementInfo("ACH_WILLING_SPIRIT", "More Than a Willing Spirit", "waydroid.png", "Return from Golgotha with a repaired waydroid and gain admittance to Grit Gate.");

	public static readonly AchievementInfo DECODING_SIGNAL = new AchievementInfo("ACH_DECODING_SIGNAL", "Decoding the Signal", "signal.png", "Travel to the Temple of the Rock and decrypt the mysterious signal.", Hidden: true);

	public static readonly AchievementInfo OMONPORCH_EARL = new AchievementInfo("ACH_OMONPORCH_EARL", "The Earl of Omonporch", "earl.png", "Strike a deal with Asphodel the Lovely for control of the Spindlegrounds.", Hidden: true);

	public static readonly AchievementInfo CALL_TO_ARMS = new AchievementInfo("ACH_CALL_TO_ARMS", "A Call to Arms", "calltoarms.png", "Defend Grit Gate from the Putus Templar.", Hidden: true);

	public static readonly AchievementInfo PAX_KLANQ = new AchievementInfo("ACH_PAX_KLANQ", "Pax Klanq, I Presume?", "klanq.png", "Deliver a message to the mushroom progidy.", Hidden: true);

	public static readonly AchievementInfo EATER_TOMB = new AchievementInfo("ACH_EATER_TOMB", "Tomb of the Eaters", "eater.png", "Travel to Brightsheol and disable the Spindle's magnetic field.", Hidden: true);

	public static readonly AchievementInfo GOLEM = new AchievementInfo("ACH_GOLEM", "The Golem", "golem.png", "Form large creature.", Hidden: true);

	public static readonly AchievementInfo RECLAMATION = new AchievementInfo("ACH_RECLAMATION", "Reclamation", "omonporch.png", "Recover the Spindlegrounds from the Putus Templar war party.", Hidden: true);

	public static readonly AchievementInfo ALEPH = new AchievementInfo("ACH_ALEPH", "Aleph", "nephil1.png", "Defeat one of the Girsh nephilim.");

	public static readonly AchievementInfo BET = new AchievementInfo("ACH_BET", "Bet", "nephil2.png", "Defeat two of the Girsh nephilim in a single playthrough.");

	public static readonly AchievementInfo GIMEL = new AchievementInfo("ACH_GIMEL", "Gimel", "nephil3.png", "Defeat three of the Girsh nephilim in a single playthrough.");

	public static readonly AchievementInfo DALET = new AchievementInfo("ACH_DALET", "Dalet", "nephil4.png", "Defeat four of the Girsh nephilim in a single playthrough.");

	public static readonly AchievementInfo HE = new AchievementInfo("ACH_HE", "He", "nephil5.png", "Defeat five of the Girsh nephilim in a single playthrough.");

	public static readonly AchievementInfo VAV = new AchievementInfo("ACH_VAV", "Vav", "nephil6.png", "Defeat six of the Girsh nephilim in a single playthrough.");

	public static readonly AchievementInfo ZAYIN = new AchievementInfo("ACH_ZAYIN", "Zayin", "nephil7.png", "Defeat all seven of the Girsh nephilim in a single playthrough.", Hidden: true);

	public static readonly AchievementInfo PACIFY_ALL = new AchievementInfo("ACH_PACIFY_ALL", "Delight in the Abundance of Peace", "pacify.png", "Pacify all seven of the Girsh nephilim in a single playthrough.", Hidden: true);

	public static readonly AchievementInfo GLITCH_OBJECT = new AchievementInfo("ACH_GLITCH_OBJECT", "The Laws of Physics Are Mere Suggestions, Vol. 3", "physics3.png", "Prove the information paradox by causing your surroundings to glitch.");

	public static readonly AchievementInfo GLITCH_SELF = new AchievementInfo("ACH_GLITCH_SELF", "Total Makeover", "static.png", "Pour a dram of pure warm static on yourself.");

	public static readonly AchievementInfo DEEP_DREAM = new AchievementInfo("ACH_DEEP_DREAM", "Dream within a Dream", "dream.png", "Enter a waking dream from within a waking dream.");

	public static readonly AchievementInfo SWOLLEN_BULB = new AchievementInfo("ACH_SWOLLEN_BULB", "Swollen Bulb", "sunslag.png", "Consume 10 drams of sunslag in a single playthrough.", "STAT_SUNSLAG_DRAMS_DRANK", 10);

	public static readonly AchievementInfo WANDER_DARKLING = new AchievementInfo("ACH_WANDER_DARKLING", "Wander Darkling in the Eternal Space", "darkling.png", "Lose your will to live.");

	public static readonly AchievementInfo SOUND_SLEEPER = new AchievementInfo("ACH_SOUND_SLEEPER", "Sound Sleeper", "sleeper.png", "Wake peacefully from twenty dreams.", "STAT_WAKING_DREAM_PEACEFUL", 20);

	public static readonly AchievementInfo SMOKE_HOOKAH = new AchievementInfo("ACH_SMOKE_HOOKAH", "Waterpipe and Rest", "hookah.png", "Smoke from a hookah.");

	public static readonly AchievementInfo ARCONAUT = new AchievementInfo("ACH_ARCONAUT", "Arconaut", "arconaut.png", "Descend 50 strata deep.");

	public static readonly AchievementInfo INFILTRATE_MECHA = new AchievementInfo("ACH_INFILTRATE_MECHA", "Get in the Robot", "robot.png", "Infiltrate a temple mecha.");

	public static readonly AchievementInfo VAPORIZED = new AchievementInfo("ACH_VAPORIZED", "Heat Death", "heat.png", "Get vaporized.", Hidden: true);

	public static readonly AchievementInfo DIE_THIRST = new AchievementInfo("ACH_DIE_THIRST", "0 for 2", "thirst.png", "Die of thirst.", Hidden: true);

	public static readonly AchievementInfo DIE_COMPANION = new AchievementInfo("ACH_DIE_COMPANION", "Oops", "oops.png", "Be killed by a companion on accident.", Hidden: true);

	public static readonly AchievementInfo DIE_NORMALITY = new AchievementInfo("ACH_DIE_NORMALITY", "Imagine a Rubber Sheet", "rubber.png", "Dash yourself on the crags of spacetime.");

	public static readonly AchievementInfo LAIRS_100 = new AchievementInfo("ACH_LAIRS_100", "Lair Cake", "cake.png", "Discover 100 lairs.", "STAT_LAIRS_VISITED", 100);

	public static readonly AchievementInfo TAU_THEN = new AchievementInfo("ACH_TAU_THEN", "Then", "then.png", "Help Chavvah complete the ritual of -elseing and sever Tau at Taproot.", Hidden: true);

	public static readonly AchievementInfo TAU_ELSE = new AchievementInfo("ACH_TAU_ELSE", "Else", "else.png", "Help Chavvah complete the ritual of -elseing and sever Tau at Taproot.", Hidden: true);

	public static readonly AchievementInfo BEY_LAH_COMEDY = new AchievementInfo("ACH_BEY_LAH_COMEDY", "The Comedy", "comedy.png", "Settle the dispute in Bey Lah in Eskhind's favor.", Hidden: true);

	public static readonly AchievementInfo BEY_LAH_HISTORY = new AchievementInfo("ACH_BEY_LAH_HISTORY", "The History", "history.png", "Settle the dispute in Bey Lah in Hindriarch Keh's favor.", Hidden: true);

	public static readonly AchievementInfo BEY_LAH_TRAGEDY = new AchievementInfo("ACH_BEY_LAH_TRAGEDY", "The Tragedy", "tragedy.png", "Settle the dispute in Bey Lah and doom the village.", Hidden: true);

	public static readonly AchievementInfo BEY_LAH_SONNET = new AchievementInfo("ACH_BEY_LAH_SONNET", "The Sonnet", "sonnet.png", "Return the crumpled sheet of paper to its intended recipient or its author.", Hidden: true);

	public static readonly AchievementInfo STARFREIGHT = new AchievementInfo("ACH_STARFREIGHT", "We Are Starfreight", "spindle.png", "Ascend the Spindle.", Hidden: true);

	public static readonly AchievementInfo KLANQ_SUN = new AchievementInfo("ACH_KLANQ_SUN", "Klanq Puff at Sun", "klanqsun.png", "Spread Klanq in space.", Hidden: true);

	public static readonly AchievementInfo CAVES_OF_QUD = new AchievementInfo("ACH_CAVES_OF_QUD", "Caves of Qud", "coq.png", "Beat the game.");

	public static readonly AchievementInfo END_COVENANT = new AchievementInfo("ACH_END_COVENANT", "Seraphic Covenant", "covenant.png", "Enter into a covenant with Resheph and help prepare Qud for the Coven's return.", Hidden: true);

	public static readonly AchievementInfo END_LAUNCH = new AchievementInfo("ACH_END_LAUNCH", "The Dusted Cosmos", "spaceship.png", "Leave the solar system on a starship.", Hidden: true);

	public static readonly AchievementInfo END_RETURN = new AchievementInfo("ACH_END_RETURN", "Seraphic Heresy", "return.png", "Spurn Resheph and return to Qud.", Hidden: true);

	public static readonly AchievementInfo END_ACCEDE = new AchievementInfo("ACH_END_ACCEDE", "The Tillage of the Noosphere", "accede.png", "Accede to Resheph's plan for purging the world of higher life.", Hidden: true);

	public static readonly AchievementInfo UNGYRE = new AchievementInfo("ACH_UNGYRE", "Ungyre", "ungyre.png", "Annul the plagues of the Gyre.", Hidden: true);

	public static readonly StatInfo STAT_BOOKS_READ = StatInfo.Create("STAT_BOOKS_READ");

	public static readonly StatInfo STAT_GAVE_REPULSIVE_DEVICE_NACHAM = StatInfo.Create("STAT_GAVE_REPULSIVE_DEVICE_NACHAM");

	public static readonly StatInfo STAT_GAVE_REPULSIVE_DEVICE_VAAM = StatInfo.Create("STAT_GAVE_REPULSIVE_DEVICE_VAAM");

	public static readonly StatInfo STAT_GAVE_REPULSIVE_DEVICE_DAGASHA = StatInfo.Create("STAT_GAVE_REPULSIVE_DEVICE_DAGASHA");

	public static readonly StatInfo STAT_GAVE_REPULSIVE_DEVICE_KAH = StatInfo.Create("STAT_GAVE_REPULSIVE_DEVICE_KAH");
}
