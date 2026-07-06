export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const preflightHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization'
    };
    const cors = {
      'Access-Control-Allow-Origin': '*',
      'Content-Type': 'application/json'
    };

    if (request.method === 'OPTIONS') {
      return new Response(null, { headers: preflightHeaders });
    }

    if (url.pathname === '/leaderboard' && request.method === 'GET') {
      const raw = await env.TETRIS_KV.get('leaderboard:top5');
      const top5 = raw ? JSON.parse(raw) : [];
      return new Response(JSON.stringify(top5), { headers: cors });
    }

    if (url.pathname === '/leaderboard' && request.method === 'POST') {
      const auth = request.headers.get('Authorization') || '';
      if (!auth) {
        return new Response(JSON.stringify({ error: 'Unauthorized' }), {
          status: 401,
          headers: cors
        });
      }

      const body = await request.json();
      const username = String(body.username || '').trim();
      const score = parseInt(body.score, 10);

      if (!username || isNaN(score)) {
        return new Response(JSON.stringify({ error: 'Invalid body' }), {
          status: 400,
          headers: cors
        });
      }

      const existingRaw = await env.TETRIS_KV.get(`score:${username}`);
      const existingScore = existingRaw !== null ? parseInt(existingRaw, 10) : null;

      if (existingScore !== null && score <= existingScore) {
        const top5Raw = await env.TETRIS_KV.get('leaderboard:top5');
        const top5 = top5Raw ? JSON.parse(top5Raw) : [];
        return new Response(JSON.stringify(top5), { headers: cors });
      }

      // New or improved score: write it
      await env.TETRIS_KV.put(`score:${username}`, String(score));

      // If this is a new player, add to the players list
      if (existingScore === null) {
        const playersRaw = await env.TETRIS_KV.get('leaderboard:players');
        const players = playersRaw ? JSON.parse(playersRaw) : [];
        if (!players.includes(username)) {
          players.push(username);
          await env.TETRIS_KV.put('leaderboard:players', JSON.stringify(players));
        }
      }

      // Re-rank: fetch all player scores
      const playersRaw = await env.TETRIS_KV.get('leaderboard:players');
      const players = playersRaw ? JSON.parse(playersRaw) : [username];
      const entries = [];
      for (const player of players) {
        const raw = await env.TETRIS_KV.get(`score:${player}`);
        if (raw !== null) {
          entries.push({ username: player, score: parseInt(raw, 10) });
        }
      }
      entries.sort((a, b) => b.score - a.score);
      const top5 = entries.slice(0, 5).map((e, i) => ({
        rank: i + 1,
        score: e.score,
        username: e.username
      }));

      await env.TETRIS_KV.put('leaderboard:top5', JSON.stringify(top5));
      return new Response(JSON.stringify(top5), { headers: cors });
    }

    return new Response('Not found', { status: 404 });
  }
};
