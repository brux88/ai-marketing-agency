# handoff/ — pacchetto rebrand weposteai.com

Questa cartella contiene tutto ciò che serve a Claude Code per applicare il nuovo brand al frontend Next.js **senza toccare la logica**.

## Struttura

```
handoff/
├── README.md                      ← questo file
├── BRAND.md                       ← istruzioni principali (LEGGERE PRIMA)
├── prompt-for-claude-code.md      ← prompt pronto da incollare in Claude Code
│
├── tokens.css                     → src/app/globals.css
├── layout.snippet.tsx             → src/app/layout.tsx
├── Logo.tsx                       → src/components/shared/Logo.tsx
├── BrandMark.tsx                  → src/components/shared/BrandMark.tsx
│
├── icon.svg                       → src/app/icon.svg
├── icon-mark.svg                  → public/brand/icon-mark.svg
└── icon-mark-filled.svg           → public/brand/icon-mark-filled.svg
```

## Come usarlo

1. **Copia questa intera cartella `handoff/`** nella root del tuo progetto `MediaCompany/frontend/` (o dove apri Claude Code)
2. Apri Claude Code nel terminale del frontend
3. Incolla il contenuto di `prompt-for-claude-code.md`
4. Claude Code leggerà `BRAND.md` e applicherà la checklist
5. A fine lavoro, cancella la cartella `handoff/` (non deve finire in produzione)

## Cosa c'è dentro (in breve)

- **BRAND.md** — la bibbia del rebrand: cosa toccare, cosa non toccare, design tokens, regole sul lime, checklist
- **tokens.css** — nuovo `globals.css` Tailwind 4 con token brand + mapping a shadcn/ui + dark mode + utilities
- **layout.snippet.tsx** — nuovo `layout.tsx` con 3 font (Instrument Serif, Geist, JetBrains Mono)
- **Logo.tsx** — componente con 5 varianti: `primary | inverted | mark | wordmark | monogram`
- **BrandMark.tsx** — solo il simbolo cursore-bolla, controllabile (animated, colori)
- **icon*.svg** — favicon e app icon

## Dopo il rebrand

Una volta che Claude Code ha finito:
1. Avvia `npm run dev`
2. Verifica almeno: landing, login, dashboard home, un editor, una pagina settings
3. Controlla in dark mode
4. Controlla su mobile (responsive del logo)

Se qualcosa non torna, aprimi la pagina che non torna e la aggiustiamo insieme.
