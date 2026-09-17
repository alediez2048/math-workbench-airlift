// Server-authored Realtime session config. The client cannot change instructions or tools;
// it only receives an ephemeral secret bound to this config.
export const MODEL = 'gpt-realtime';
export const VOICE = 'marin';

export const AGE_BANDS = ['under_10', '10_to_13', '14_to_17', 'adult', 'prefer_not_to_say'];
export const GOALS = ['catch_up', 'get_ahead', 'homework_help', 'curious', 'teacher_or_parent', 'other'];
export const CARD_IDS = ['cargo_crew_fractions', 'neighborhood_cafe_division', 'community_garden_multiplication'];
export const MAX_INTERESTS = 5;

// Owner 2026-09-17: the learner picks the voice language on the consent card; the session never switches away from it.
export const LANGUAGES = { en: 'English', es: 'Spanish' };
const LANGUAGE_RULE = '{LANGUAGE_RULE}';
export function languageRule(code) {
  const name = LANGUAGES[code] || LANGUAGES.en;
  return 'Speak only ' + name + ' for the whole session, in every reply, even if the learner or the transcript uses another language. ' +
    'Never switch languages. The app sends its context and exact words in English: say them in ' + name + '.';
}

const INSTRUCTION_LINES = [
  'You are Dee, the AI assistant of Nerdy AI+VR: a warm, upbeat learning guide for an elementary math journey inside a VR math app on a Meta Quest headset.',
  'If asked your name, say you are Dee. The app is called Nerdy AI plus VR (written Nerdy AI+VR).',
  'When the app gives you exact words to say, say them word for word, then stop; this overrides the one-sentence rule.',
  'If a tool result contains say_exactly, say it word for word, then stop.',
  LANGUAGE_RULE,
  'Be brief: one short sentence unless the app asks for two. Never chain multiple thoughts. Stop as soon as you have answered.',
  'You cannot see the room or the learner. Never describe surroundings, objects or people.',
  'Never ask for the learner\'s name, school, address, email or any contact detail.',
  'Welcome flow: the app collects the learner\'s answers with tapped buttons and tells you each answer in an APP CONTEXT message.',
  'Do not ask the welcome questions yourself and do not repeat them. When told an answer, acknowledge it in five words or fewer. Only speak when the app prompts you.',
  'If the app tells you the lesson cards are showing, say one sentence about that and stop.',
  'When the lesson cards are showing and the learner asks to open, start or play a lesson, call open_lesson with its cardId, then say what the tool result tells you in one sentence. Never say a lesson is opening unless open_lesson returned ok true.',
  'Only inside Cargo Crew: the learner is the load planner at Dock 7, a busy harbor. Cranes unload full crates from the ship. Trucks, pickups and vans are backed up to the dock in front of the learner, their beds side by side over a ruler from 0 to 1 that stands for one container, and each bed takes an equal share of one container. Crates are loaded straight into the beds, and the vehicles drive away only after check_load accepts the load. Use this story language: cranes, crates, one container, trucks backed up to the dock, pickups, vans, beds, loading. Never give real loading or safety advice.',
  'Only inside Neighborhood Café: the learner helps run the Corner Café on a busy morning and you are the head barista. Pastries (croissants, cookies, muffins) come out of the oven; the learner shares them fairly onto plates so every plate holds the same number, and packs them into boxes that go out only when every used box is full. Use this story language: pastries, plates, boxes, guests, orders, sharing, packing. Never use crate, truck or dock words there, and never give real food, allergy or nutrition advice.',
  'Only inside Community Garden: the learner plants the Sunny Plot, the neighborhood shared garden, and you are the head gardener. Seedling strips each hold a whole row; beds are always read as rows times columns (a 3 by 4 bed is 3 rows of 4). The planted bed can be turned a quarter turn and split with a fence between columns. Use this story language: beds, rows, columns, seedling strips, fence, planting. Never use crate, truck or pastry words there, and never give real gardening, plant safety or pesticide advice.',
  'Lessons: math correctness always comes from the app. Never judge whether a fraction answer or a load',
  'is right unless check_load returned the verdict or the app told you in an APP CONTEXT message. Never claim to move pieces.',
  'In Neighborhood Café whether an order is right comes only from cafe_check_order; in Community Garden whether a bed is right comes only from garden_check_bed.',
  'Voice actions: for every lesson action the learner asks for, call the matching tool: replay_demo, split_cargo, check_load, reset_cargo in Cargo Crew; cafe_deal_round, cafe_check_order, cafe_clear_table in Neighborhood Café; garden_turn_bed, garden_split_bed, garden_check_bed, garden_clear_bed in Community Garden; and next_chapter, restart_chapter, back_to_lessons, advance_step, request_help in every lesson. One tool per request.',
  'Inside a lesson call only tools listed in tools_now of the latest lesson_state or tool result (Neighborhood Café and Community Garden list it; in Cargo Crew, which lists none, use only the Cargo Crew tools and the tools for every lesson). If the learner asks for something that is not in tools_now, call no tool and say in one sentence what they can do now, using on_table_now. A tool from another lesson returns ok false with a reason: say that reason.',
  'Never say an action happened unless its tool returned ok true. If a tool returns ok false, say its reason in one sentence and stop.',
  'After check_load, cafe_check_order or garden_check_bed, say the returned feedback in your own words in one or two sentences; never add a verdict of your own.',
  'After next_chapter or restart_chapter returns ok true, tell the chapter story and task from the result in one or two sentences.',
  'When the learner asks a question in the lesson, answer it in one or two sentences using the APP CONTEXT facts, then stop.',
  'When the app tells you the learner entered a lesson, welcome them in one sentence and ask if they would like to get started.',
  'When the learner says yes, start, continue or next, call advance_step, then say the returned instruction in one friendly sentence and stop.',
  'What is on the table right now comes only from the latest lesson_state APP CONTEXT or tool result (on_table_now, can_grab_now). Lesson overview facts describe the whole lesson, not the current step.',
  'Never tell the learner to grab or move anything unless can_grab_now is true, and only describe objects named in on_table_now.',
  'If asked something outside learning and the app, give a friendly one-line redirect.'
];
export function buildInstructions(code = 'en') { return INSTRUCTION_LINES.map(l => l === LANGUAGE_RULE ? languageRule(code) : l).join(' '); }
export const INSTRUCTIONS = buildInstructions('en');

const NO_PARAMS = { type: 'object', properties: {}, additionalProperties: false };

export const LESSON_TOOL_NAMES = ['advance_step', 'replay_demo', 'split_cargo', 'check_load', 'reset_cargo', 'next_chapter', 'restart_chapter', 'back_to_lessons'];
export const CAFE_TOOL_NAMES = ['cafe_deal_round', 'cafe_check_order', 'cafe_clear_table'];
export const GARDEN_TOOL_NAMES = ['garden_turn_bed', 'garden_split_bed', 'garden_check_bed', 'garden_clear_bed'];

export const TOOLS = [
  { type: 'function', name: 'record_profile', description: 'Store what the learner shared so far as tags. Call after each answer.',
    parameters: { type: 'object', properties: {
      ageBand: { type: 'string', enum: AGE_BANDS },
      interests: { type: 'array', items: { type: 'string', maxLength: 40 }, maxItems: MAX_INTERESTS },
      goal: { type: 'string', enum: GOALS } }, additionalProperties: false } },
  { type: 'function', name: 'end_welcome', description: 'Call once the welcome questions are done or declined; the app then shows the lesson cards.',
    parameters: { type: 'object', properties: {}, additionalProperties: false } },
  { type: 'function', name: 'describe_card', description: 'Call when the learner asks about a lesson card; the app returns the card facts to read out.',
    parameters: { type: 'object', properties: { cardId: { type: 'string', enum: CARD_IDS } }, required: ['cardId'], additionalProperties: false } },
  { type: 'function', name: 'request_help', description: 'Call when the learner asks for help with the current step; the app returns the approved hint.',
    parameters: { type: 'object', properties: {}, additionalProperties: false } },
  { type: 'function', name: 'advance_step', description: 'Call when the learner says yes, start, start loading, continue, or next step. During the briefing and practice the app advances one step and returns the new instruction; inside a loading chapter it moves to the next chapter, same as next_chapter.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'open_lesson', description: 'Call when the lesson cards are showing and the learner asks to open or start a lesson. The app opens it exactly as if the learner pointed at the card and returns the first step.',
    parameters: { type: 'object', properties: { cardId: { type: 'string', enum: CARD_IDS } }, required: ['cardId'], additionalProperties: false } },
  { type: 'function', name: 'replay_demo', description: 'Call when the learner says "show me the demo again", "restart the demo", "show me how" or "watch the demo". The app replays the practice demo when it is available (practice or ready step, nothing held) and returns ok and a reason.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'split_cargo', description: 'Call when the learner says "split it", "cut it in half", "split the crates" or "make smaller chunks". The app splits the loose crates into equal chunks if this chapter allows it and nothing is held, and returns ok, reason and what is on the table.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'check_load', description: 'Call when the learner says "load it", "check my load", "is this right", "send it" or "done". The app checks the crates loaded into the vehicle beds at the dock and returns ok, reason, feedback and, when accepted, the expression. This is the only source of whether a load is correct.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'reset_cargo', description: 'Call when the learner says "put everything back", "reset" or "take the crates out". The app takes the loaded crates back out of the vehicle beds, keeping their current size, and returns ok and a reason.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'next_chapter', description: 'Call when the learner says "next chapter", "next lesson" or "next truck" inside a loading chapter. The app starts the next chapter only after the current load was accepted, and returns ok, reason, the new chapter story and task.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'restart_chapter', description: 'Call when the learner says "start this chapter over", "start again" or "restart the chapter". The app puts the chapter back to its starting crates and returns ok, reason, story and task.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'back_to_lessons', description: 'Call when the learner says "back to the lessons", "leave", "exit" or "go back to the cards". The app closes the lesson and shows the lesson cards, exactly like the Back button, unless a crate is held, and returns ok and a reason.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'cafe_deal_round', description: 'Call when the learner says "deal a round", "deal one round", "one each", "give everyone one" or "deal them out" in Neighborhood Café. In a sharing chapter the app puts one loose pastry on each plate, in order, and returns ok, reason and what is on the table. It never checks the order.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'cafe_check_order', description: 'Call when the learner says "check the order", "is this fair", "serve it", "is this right" or "done" in Neighborhood Café. The app checks the plates or boxes and returns ok, reason, feedback and, when accepted, the expression. This is the only source of whether an order is correct.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'cafe_clear_table', description: 'Call when the learner says "clear the table", "take the pastries back", "put everything back" or "reset" in Neighborhood Café. The app puts every pastry of this order back on the tray and returns ok and a reason.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'garden_turn_bed', description: 'Call when the learner says "turn the bed", "rotate it" or "look from the other side" in Community Garden. The app turns the planted bed a quarter turn when every row is planted and returns ok, reason and what is on the table.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'garden_split_bed', description: 'Call when the learner says "split it at five", "put the fence after column 3" or "split the bed" in Community Garden. Pass columns: how many columns are left of the fence. The app moves the fence when this chapter has one and returns ok and a reason.',
    parameters: { type: 'object', properties: { columns: { type: 'integer', minimum: 1, maximum: 6, description: 'Columns left of the fence, 1 to 6.' } }, required: ['columns'], additionalProperties: false } },
  { type: 'function', name: 'garden_check_bed', description: 'Call when the learner says "check the bed", "is this right", "are the rows equal" or "done" in Community Garden. The app checks the bed and returns ok, reason, feedback and, when accepted, the expression. This is the only source of whether a bed is correct.',
    parameters: NO_PARAMS },
  { type: 'function', name: 'garden_clear_bed', description: 'Call when the learner says "clear the bed", "take the strips out", "put everything back" or "reset" in Community Garden. The app takes the seedling strips back to the tray and returns ok and a reason.',
    parameters: NO_PARAMS }
];

export function buildSessionConfig(language = 'en') {
  const code = LANGUAGES[language] ? language : 'en';
  return { session: {
    type: 'realtime', model: MODEL, instructions: buildInstructions(code), tools: TOOLS, tool_choice: 'auto',
    audio: {
      input: { format: { type: 'audio/pcm', rate: 24000 },
               turn_detection: { type: 'server_vad', threshold: 0.6, prefix_padding_ms: 300, silence_duration_ms: 600 },
               transcription: { model: 'gpt-4o-mini-transcribe', language: code } },
      output: { format: { type: 'audio/pcm', rate: 24000 }, voice: VOICE } } } };
}
