# Prompt pronto da incollare in Claude Code

Copia-incolla questo nella tua conversazione con Claude Code (nel terminale del progetto `frontend`).

---

Sto applicando un rebrand al frontend Next.js. Ho preparato tutti i file in una cartella `handoff/` che troverai nella root di questa repo (o nella cartella che ti indico).

**Leggi `handoff/BRAND.md` integralmente prima di fare qualsiasi cosa.** Contiene:
- la lista precisa di cosa puoi toccare e cosa NON puoi toccare
- i nuovi design tokens
- il mapping verso shadcn/ui esistente
- la checklist ordinata di esecuzione

Poi esegui la checklist step-by-step, facendo **un commit per ogni step** con messaggio chiaro (es. `brand: swap globals.css tokens`, `brand: add Logo component`, ecc.).

**Vincoli rigidi:**
- Non modificare logica, routing, API, hook, store, provider, validazioni
- Non rinominare componenti, props, route o tipi
- Non toccare `backend/`, `mobile/`, file di test
- Se incontri una stringa visibile `"WePostAI"` o `"wepost.ai"` sostituiscila con `<Logo />` (se è un header/nav/footer) o con la stringa `"weposteai.com"` (se è testo puro come metadata o alt)
- Dopo ogni step esegui `npm run build` e fermati se fallisce

Alla fine fammi un recap breve di:
1. file modificati
2. file creati
3. eventuali punti dove hai dovuto adattare più di quanto previsto
4. eventuali stringhe "WePostAI" che hai trovato e come le hai risolte

Quando sei pronto, inizia.
