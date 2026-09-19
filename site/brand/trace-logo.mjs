import fs from 'node:fs';
import potrace from 'potrace';

const source = new URL('./trace-mask.png', import.meta.url);
const output = new URL('./nerdy-ai-vr-logo.svg', import.meta.url);
const metadata = new URL('./trace-meta.json', import.meta.url);
const { width, height } = JSON.parse(fs.readFileSync(metadata, 'utf8'));

potrace.trace(source.pathname, { turdSize: 2, optCurve: true, alphaMax: 1.1, threshold: 128 }, (error, svg) => {
  if (error) throw error;
  const inner = svg.match(/<path[^>]+d="([^"]+)"[^>]*>/);
  if (!inner) throw new Error('Trace yielded no vector path');
  const gradient = `<defs><linearGradient id="spectrum" x1="0" x2="1" y1="0" y2="0"><stop offset="0" stop-color="#42bd80"/><stop offset=".46" stop-color="#42bd80"/><stop offset=".54" stop-color="#2cc5e4"/><stop offset=".67" stop-color="#13c4d5"/><stop offset=".72" stop-color="#b08afa"/><stop offset=".77" stop-color="#b08afa"/><stop offset=".80" stop-color="#ffc052"/><stop offset=".86" stop-color="#f34ec3"/><stop offset="1" stop-color="#bf53ed"/></linearGradient></defs>`;
  const result = `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}" role="img" aria-labelledby="title"><title id="title">Nerdy AI plus VR</title>${gradient}<path fill="url(#spectrum)" fill-rule="evenodd" d="${inner[1]}"/></svg>`;
  fs.writeFileSync(output, result);
  console.log(`Saved ${output.pathname} (${width}x${height})`);
});
