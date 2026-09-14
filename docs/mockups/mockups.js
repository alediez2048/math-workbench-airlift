'use strict';
// Standalone schematic reference. No Unity, networking, model calls, or learner data.
const views = {
  arrival: { chapter:'ARRIVAL / PLACE THE WORKBENCH', title:'A workshop within reach.', description:'A calm beginning: one task, a visible whole, and a little aircraft waiting for its cargo.', notes:['The room remains visible around a stable virtual bench. No required room scan.','The whole and instruction sit together. Optional height adjustment happens before grabbing.','Aircraft and cargo provide context outside the essential reaching area.'], invariant:'Place once, then world-lock. Turning your head must not move the table.', prompt:'Every strap starts with one whole.', sub:'Align the unit strap with the ruler.' },
  build: { chapter:'CHAPTER 01 / BUILD · T03', title:'Make three fourths.', description:'The pieces, the ruler, and the symbolic statement describe the same quantity. A wrong length stays available to inspect.', notes:['Each quarter occupies exactly two of the eight underlying cells.','All safe lengths up to one whole receive the same placement assistance.','Only Submit evaluates the answer. Grabbing is not a correctness signal.'], invariant:'The whole stays fixed. Three fourth-pieces cover 3/4 of it.', prompt:'Build a strap that is three fourths long.', sub:'Place equal pieces. Then submit your arrangement.' },
  equivalence: { chapter:'CHAPTER 02 / COMPARE · T04', title:'More parts. Same length.', description:'Splitting each fourth in two doubles the part count without changing the strap. The student constructs the statement separately.', notes:['Both lanes share zero, one, and the same endpoint.','Six eighths remains labeled 6/8; normalization must not erase the learning representation.','Independent symbol fields can be wrong without changing the physical model.'], invariant:'3/4 and 6/8 must remain exactly the same length.', prompt:'Split each fourth into two equal parts.', sub:'What changed? What stayed the same?' },
  compare: { chapter:'CHAPTER 02 / COMPARE · T05', title:'A mistake becomes a question.', description:'The learner commits an inequality and an explicit reason. Authored support responds to that evidence, not an inferred learning style.', notes:['The prediction can be wrong; no lane secretly filters for a correct answer.','The same wrong inequality can lead to different support when the chosen reason differs.','The arrangement stays visible for repair. These are local fixtures, not live AI.'], invariant:'When using half as a benchmark, compare the excess above half: 2/8 versus 1/8.', prompt:'Which strap reaches farther?', sub:'Compare three fourths and five eighths of the same whole.' },
  departure: { chapter:'CHAPTER 03 / APPLY · PAYOFF', title:'The math does useful work.', description:'After the fresh-value task, the straps secure the cargo. A short aircraft departure closes the experience without a score or a mastery claim.', notes:['This scene follows T06; it is not a substitute for the near-transfer task.','Motion belongs to the miniature aircraft, never the player camera.','Replay is practice. First-attempt evidence is kept distinct from supported completion.'], invariant:'A correct completed task unlocks the payoff. There are no lives, coins, or countdowns.', prompt:'Your straps are ready for the cargo.', sub:'Different partitions. A dependable fit.' }
};
let active='arrival', quarters=3, numerator=6, denominator=8, eqOperator='=', comparison='<', reason='more_parts', benchmark=false, support='', submitted=false;
const scene=document.getElementById('scene');
const feedback=document.getElementById('feedback');
const txt=(x,y,s,cls='label',extra='')=>`<text x="${x}" y="${y}" class="${cls}" ${extra}>${s}</text>`;
const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');
const marker=(x,y,n)=>`<circle cx="${x}" cy="${y}" r="12" fill="#f7edcf" stroke="#849b8e"/>${txt(x,y+5,n,'small','text-anchor="middle"')}`;
function plane(x,y,scale=1){return `<g transform="translate(${x} ${y}) scale(${scale})"><ellipse cx="0" cy="57" rx="86" ry="13" fill="#bccabc" opacity=".55"/><path d="M-69,0 L46,-10 Q79,-7 84,8 Q68,23 42,20 L-67,10 Z" fill="#f5f1d9" stroke="#325b5b" stroke-width="2"/><path d="M-5,5 L-49,48 L-20,48 L31,12 Z" fill="#dcaa50" stroke="#325b5b" stroke-width="2"/><path d="M-2,2 L-25,-34 L0,-32 L33,3 Z" fill="#ebbe6a" stroke="#325b5b" stroke-width="2"/><path d="M-57,0 L-71,-28 L-51,-26 L-34,2" fill="#267a77"/><rect x="40" y="-5" width="15" height="10" rx="3" fill="#284f56"/><path d="M85,-14 L85,32" stroke="#345655" stroke-width="5" stroke-linecap="round"/><circle cx="41" cy="29" r="8" fill="#365856"/><circle cx="-42" cy="20" r="7" fill="#365856"/></g>`;}
function lane(y,parts,filled,label,color='#267a77'){
  const x=244,w=480,h=45,pw=w/parts;
  let svg=txt(x,y-12,label,'small')+`<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="4" fill="#e0e4d7" stroke="#869e90"/>`;
  // Adjacent pieces share edges: no hidden gaps or length distortion.
  for(let i=0;i<filled;i++)svg+=`<rect data-piece="true" x="${x+i*pw}" y="${y}" width="${pw}" height="${h}" fill="${color}" stroke="#f3f5e6" stroke-width="1"/>`+txt(x+(i+.5)*pw,y+29,`1/${parts}`,'small white','text-anchor="middle"');
  svg+=`<line x1="${x}" y1="${y+h+12}" x2="${x+w}" y2="${y+h+12}" stroke="#769285"/>`;
  for(let i=0;i<=8;i++)svg+=`<line x1="${x+i*60}" y1="${y+h+8}" x2="${x+i*60}" y2="${y+h+17}" stroke="#769285"/>`;
  svg+=txt(x,y+h+37,'0','small','text-anchor="middle"')+txt(x+w/2,y+h+37,'1/2','small','text-anchor="middle"')+txt(x+w,y+h+37,'1 whole','small','text-anchor="middle"');
  return svg;
}
function draw(){
  const v=views[active];
  let body=`<title id="scene-title">${esc(v.title)} — schematic design reference</title><desc id="scene-desc">${esc(v.description)} Equal whole lanes show exact fractional lengths; the room and workbench are schematic, not headset footage.</desc>
  <defs><linearGradient id="room" x2="0" y2="1"><stop stop-color="#e3e9df"/><stop offset="1" stop-color="#c8d8c9"/></linearGradient><linearGradient id="floor" x2="0" y2="1"><stop stop-color="#d0dcca"/><stop offset="1" stop-color="#b2c7b8"/></linearGradient></defs>
  <rect width="1000" height="650" fill="url(#room)"/><path d="M0 0H165V364L0 461Z" fill="#d6e0d2"/><path d="M1000 0H896V362L1000 445Z" fill="#d6dfd0"/><path d="M0 410L165 337H896L1000 411V650H0Z" fill="url(#floor)"/>
  <g opacity=".65"><rect x="45" y="64" width="112" height="215" rx="3" fill="#eff2df"/><path d="M101 64V279M45 171H157" stroke="#c2d1bf" stroke-width="6"/><path d="M165 337L0 650M896 337L1000 650M390 337L319 650M672 337L741 650" stroke="#b6c8b7"/></g>
  <g opacity=".35"><rect x="779" y="237" width="170" height="103" rx="16" fill="#aabfb1"/><rect x="773" y="287" width="180" height="77" rx="14" fill="#9fb5a7"/><path d="M790 356V389M934 356V390" stroke="#78988b" stroke-width="8"/></g>
  <ellipse cx="492" cy="572" rx="361" ry="45" fill="#7d9c88" opacity=".28"/>
  <path d="M207 431L197 613H219L239 431M780 431L792 613H814L803 431" fill="#557a6c"/>
  <path d="M225 244H786L928 523H86Z" fill="#f3f0dc" stroke="#849a80" stroke-width="2"/><path d="M86 523H928V546H86Z" fill="#b6bca2"/><path d="M126 506H887" stroke="#d4d5bf"/>
  ${active==='departure'?plane(838,219,.78):plane(796,230,.66)}
  <g><path d="M735 284L779 273L812 288L769 301Z" fill="#d0a66b"/><path d="M735 284V320L769 338V301Z" fill="#b48d59"/><path d="M769 301L812 288V322L769 338Z" fill="#c09a60"/><path d="M756 279V330M791 282V330" stroke="#386f68" stroke-width="7"/></g>
  <rect x="210" y="92" width="555" height="107" rx="10" fill="#f8f5e7" stroke="#b6c5b3"/>${txt(237,129,v.prompt,'big')}${txt(237,161,v.sub,'small quiet')}
  ${marker(194,93,1)}${marker(197,311,2)}${marker(811,201,3)}`;
  if(active==='arrival')body+=lane(326,1,1,'UNIT STRAP · our reference whole')+txt(484,476,'One whole. Equal parts. Your workbench.','label','text-anchor="middle"');
  if(active==='build')body+=lane(322,4,quarters,'YOUR STRAP · equal fourths')+`<rect x="244" y="445" width="480" height="46" rx="6" fill="#e5e9dc"/>`+txt(484,475,quarters?`${Array(quarters).fill('1/4').join(' + ')} = ${quarters}/4`:'0 fourths placed','label','text-anchor="middle"');
  if(active==='equivalence')body+=lane(282,4,3,'BEFORE · 3 fourths')+lane(397,8,6,'AFTER · 6 eighths')+`<line x1="604" y1="277" x2="604" y2="446" stroke="#315e59" stroke-width="2" stroke-dasharray="4 5"/>`+txt(748,345,'×2 parts','small')+txt(748,369,'same length','small');
  if(active==='compare'){
    const subdivided=support==='common_endpoint'||support==='equal_parts'||support==='benchmark_half';
    body+=lane(278,subdivided?8:4,subdivided?6:3,subdivided?'STRAP A · 3/4 = 6/8':'STRAP A · 3/4')+lane(394,8,5,'STRAP B · 5/8');
    if(benchmark||support==='benchmark_half')body+=`<path d="M484 266V443" stroke="#ac6b24" stroke-width="3" stroke-dasharray="6 5"/>`+txt(484,251,'half benchmark','small','text-anchor="middle"');
    if(support==='benchmark_half')body+=`<path d="M484 329H604M484 445H544" stroke="#ba702b" stroke-width="5"/>`+txt(657,311,'+2/8','label')+txt(595,427,'+1/8','label');
    else if(support==='equal_parts')body+=txt(794,377,'Count equal','small','text-anchor="middle"')+txt(794,399,'eighth-pieces.','small','text-anchor="middle"');
    else if(support==='common_endpoint')body+=`<path d="M604 274V440M544 274V440" stroke="#4e716a" stroke-dasharray="4 6"/>`;
  }
  if(active==='departure')body+=lane(322,8,4,'COMPLETED STRAP · 1/2 = 4/8')+txt(484,474,'The cargo is secured. Ready for departure.','label','text-anchor="middle"');
  body+=`<path d="M126 478 Q111 454 132 433 Q148 425 157 442 L171 472 Q170 485 153 491Z" fill="#eceddf" stroke="#66877c" stroke-width="2"/><ellipse cx="138" cy="440" rx="15" ry="10" fill="#d6ddcb" stroke="#66877c"/><path d="M872 478 Q887 454 866 433 Q850 425 841 442 L827 472 Q828 485 845 491Z" fill="#eceddf" stroke="#66877c" stroke-width="2"/><ellipse cx="860" cy="440" rx="15" ry="10" fill="#d6ddcb" stroke="#66877c"/>`;
  body+=txt(493,583,active==='compare'&&submitted?'GUIDED SUPPORT · AUTHOR-DEFINED FIXTURE':'SCHEMATIC FIRST-PERSON VIEW · NOT TO PHYSICAL SCALE','small quiet','text-anchor="middle"');
  scene.innerHTML=body;
}
const select=(id,label,options,value)=>`<label>${label}<select id="${id}">${options.map(([v,t])=>`<option value="${esc(v)}" ${String(v)===String(value)?'selected':''}>${esc(t)}</option>`).join('')}</select></label>`;
function controls(){
  const box=document.getElementById('controls');
  if(active==='arrival')box.innerHTML='<p class="context">One-controller baseline · seated or standing</p><button class="primary" id="begin">Inspect the build task →</button>';
  if(active==='build')box.innerHTML='<button id="remove">Remove a fourth</button><button id="add">Add a fourth</button><button class="primary" id="submit">Submit arrangement</button><button id="reset">Reset reference</button>';
  if(active==='equivalence')box.innerHTML='<p class="context">Your statement: 3/4</p>'+select('eq-operator','Relationship',[['=','='],['<','<'],['>','>']],eqOperator)+select('numerator','Numerator',[1,2,3,4,5,6,7,8].map(x=>[x,x]),numerator)+select('denominator','Denominator',[2,4,8].map(x=>[x,x]),denominator)+'<button class="primary" id="submit">Submit statement</button><button id="reset">Reset reference</button>';
  if(active==='compare')box.innerHTML='<p class="context">Your statement: 3/4</p>'+select('comparison','Relationship',[['<','<'],['=','='],['>','>']],comparison)+'<p class="context">5/8</p>'+select('reason','My reason',[['more_parts','More parts means more length'],['same_endpoint','The aligned endpoints show the length'],['benchmark','I compared the excess above half'],['unsure','Not sure yet']],reason)+`<button id="benchmark" aria-pressed="${benchmark}">${benchmark?'Half benchmark shown':'Use half benchmark'}</button><button class="primary" id="submit">Submit comparison</button><button id="reset">Reset reference</button>`;
  if(active==='departure')box.innerHTML='<p class="context">Payoff follows successful T06 completion.</p><button class="primary" id="replay">Replay reference →</button>';
  box.querySelector('#begin')?.addEventListener('click',()=>setView('build'));
  box.querySelector('#replay')?.addEventListener('click',()=>setView('arrival'));
  box.querySelector('#add')?.addEventListener('click',()=>{quarters=Math.min(4,quarters+1);feedback.textContent='Placement changed. Submit when you are ready.';draw();updateBuildButtons();});
  box.querySelector('#remove')?.addEventListener('click',()=>{quarters=Math.max(0,quarters-1);feedback.textContent='Placement changed. The whole has not changed.';draw();updateBuildButtons();});
  box.querySelector('#reset')?.addEventListener('click',()=>setView(active));
  for(const id of ['numerator','denominator','eq-operator','comparison','reason'])box.querySelector('#'+id)?.addEventListener('change',e=>{
    if(id==='numerator')numerator=Number(e.target.value);
    if(id==='denominator')denominator=Number(e.target.value);
    if(id==='eq-operator')eqOperator=e.target.value;
    if(id==='comparison')comparison=e.target.value;
    if(id==='reason')reason=e.target.value;
    feedback.textContent='Your statement changed. The physical quantities stay the same.';
  });
  box.querySelector('#benchmark')?.addEventListener('click',()=>{benchmark=true;controls();draw();feedback.textContent='Benchmark requested: half is marked on both lanes. Compare what remains beyond it.';});
  box.querySelector('#submit')?.addEventListener('click',submit);
  updateBuildButtons();
}
function updateBuildButtons(){if(active==='build'){document.getElementById('add').disabled=quarters===4;document.getElementById('remove').disabled=quarters===0;}}
function submit(){
  if(active==='build')feedback.textContent=quarters===3?'The three equal fourth-pieces reach 3/4 of the whole. This task can advance.':'Your strap is '+quarters+'/4 long. Compare its endpoint with three fourths, then adjust the pieces.';
  if(active==='equivalence')feedback.textContent=(eqOperator==='='&&numerator===6&&denominator===8)?'The lengths match: 3/4 = 6/8. Both the numerator and denominator doubled.':'The model still shows six eighths with the same length as three fourths. Check each field in your statement.';
  if(active==='compare'){
    const correct=comparison==='>'&&(reason==='same_endpoint'||(reason==='benchmark'&&benchmark));
    submitted=!correct;
    if(correct){support='';feedback.textContent='The comparison and selected explanation agree: 3/4 > 5/8. This task can advance.';}
    else if(comparison==='>'){support='common_endpoint';feedback.textContent='The inequality fits. Use the aligned model to choose a supporting explanation.';}
    else if(reason==='more_parts'){support='equal_parts';feedback.textContent='Guided support: compare equal eighth-pieces. How many fit each strap? Your arrangement remains available for repair.';}
    else if(reason==='benchmark'&&benchmark){support='benchmark_half';feedback.textContent='Guided support: both straps exceed half. Compare the extra lengths: 2/8 and 1/8. Then revise your statement.';}
    else{support='common_endpoint';feedback.textContent='Guided support: align the same whole from zero and compare endpoints. There is not enough evidence to choose a more specific scaffold.';}
    draw();
  }
}
function setView(next){
  active=next;quarters=3;numerator=6;denominator=8;eqOperator='=';comparison='<';reason='more_parts';benchmark=false;support='';submitted=false;
  const v=views[active];
  for(const [id,value] of [['chapter',v.chapter],['view-title',v.title],['view-description',v.description],['invariant',v.invariant]])document.getElementById(id).textContent=value;
  document.getElementById('annotations').innerHTML=v.notes.map(n=>`<li>${esc(n)}</li>`).join('');
  document.querySelectorAll('[data-view]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.view===active)));
  feedback.textContent=active==='compare'?'A deliberately incorrect prediction is loaded. Submit it to inspect repair feedback.':'Design reference only. Explore this state with the controls above.';
  controls();draw();
}
document.querySelectorAll('[data-view]').forEach(b=>b.addEventListener('click',()=>setView(b.dataset.view)));
setView('arrival');
