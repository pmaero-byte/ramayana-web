// Railway deploys the static game from this folder.
// Start command serves the project on the port Railway assigns via $PORT.
import http from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { extname, join, normalize } from 'node:path';

const PORT = Number(process.env.PORT) || 8765;
const HOST = '0.0.0.0';
const ROOT = process.cwd();

const MIME = {
  '.html': 'text/html; charset=utf-8',
  '.js':   'application/javascript; charset=utf-8',
  '.mjs':  'application/javascript; charset=utf-8',
  '.css':  'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.png':  'image/png',
  '.jpg':  'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.svg':  'image/svg+xml',
  '.webp': 'image/webp',
  '.ico':  'image/x-icon',
  '.wasm': 'application/wasm',
  '.txt':  'text/plain; charset=utf-8',
};

const safeJoin = (rel) => {
  const safe = normalize(rel).replace(/^\/+/, '');
  const full = join(ROOT, safe);
  if (!full.startsWith(ROOT)) return null;
  return full;
};

const server = http.createServer(async (req, res) => {
  try {
    const url = new URL(req.url, `http://${req.headers.host}`);
    let pathname = decodeURIComponent(url.pathname);
    if (pathname === '/' || pathname.endsWith('/')) {
      pathname += 'index.html';
    }
    const filePath = safeJoin(pathname);
    if (!filePath) {
      res.writeHead(403); res.end('Forbidden'); return;
    }
    const s = await stat(filePath).catch(() => null);
    if (!s || !s.isFile()) {
      // Fallback: try /index.html for SPA-style routes
      const idx = safeJoin('/index.html');
      const idxStat = idx ? await stat(idx).catch(() => null) : null;
      if (!idxStat) { res.writeHead(404); res.end('Not found'); return; }
      const body = await readFile(idx);
      res.writeHead(200, { 'Content-Type': MIME['.html'] });
      res.end(body);
      return;
    }
    const body = await readFile(filePath);
    const type = MIME[extname(filePath).toLowerCase()] || 'application/octet-stream';
    // Cache static assets; always revalidate index.html for the boot-watchdog
    const cache = extname(filePath) === '.html' ? 'no-cache' : 'public, max-age=600';
    res.writeHead(200, {
      'Content-Type': type,
      'Cache-Control': cache,
      'Content-Length': body.length,
    });
    res.end(body);
  } catch (err) {
    console.error(err);
    res.writeHead(500); res.end('Server error');
  }
});

server.listen(PORT, HOST, () => {
  console.log(`ramayana-web serving ${ROOT} on http://${HOST}:${PORT}`);
});
