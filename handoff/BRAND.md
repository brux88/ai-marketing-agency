# weposteai.com — Brand Handoff per Claude Code

**Scopo di questo documento**: adattare il frontend Next.js esistente al nuovo brand **weposteai.com** senza toccare logica, routing, API, autenticazione o componenti funzionali. Solo livello visivo + wordmark + tokens.

---

## ⛔ Cosa NON toccare

- `src/app/(auth)/*`, `src/app/(dashboard)/*`, `src/app/(marketing)/*` → NON modificare la logica di pagina
- `src/components/dashboard/*`, `src/components/auth/*`, `src/components/agency/*` → NON modificare struttura o props
- `src/lib/*` (api, hooks, providers, signalr, stores, validations) → NON toccare
- `src/types/*`, `backend/*`, `mobile/*` → fuori scope
- Qualsiasi file `.test.*`, `.spec.*`, configurazione CI → fuori scope
- **Non rinominare** componenti, props, route, endpoint, variabili di stato

## ✅ Cosa modificare

1. `src/app/globals.css` — sostituire i token con quelli del nuovo brand (vedi `tokens.css` in questa cartella)
2. `src/app/layout.tsx` — sostituire font `Geist + Geist_Mono` con `Instrument_Serif + Geist + JetBrains_Mono` (vedi `layout.snippet.tsx`)
3. `src/app/icon.svg` — sostituire con il nuovo mark (file fornito: `icon.svg`)
4. `public/apple-icon.png` (opzionale) — rigenerare da `icon-mark-filled.svg` a 512×512

## ➕ File da aggiungere

1. `src/components/shared/Logo.tsx` — componente logo (vedi `Logo.tsx` qui)
2. `src/components/shared/BrandMark.tsx` — solo il mark SVG come componente React (vedi `BrandMark.tsx`)
3. `public/brand/icon-mark.svg` — mark outline
4. `public/brand/icon-mark-filled.svg` — mark per app icon

---

## 🎨 Design tokens

Il brand si regge su **pochi token, usati con disciplina**. Li aggiunge `globals.css`.

| Token | Valore | Uso |
|---|---|---|
| `--color-ink` | `oklch(0.15 0 0)` / `#0A0A0B` | testo primario, fondi scuri |
| `--color-ink-2` | `oklch(0.22 0 0)` / `#18181B` | surface dark |
| `--color-paper` | `oklch(0.98 0.005 85)` / `#FAFAF7` | sfondo base |
| `--color-paper-2` | `oklch(0.95 0.008 85)` / `#F2F1EC` | surface light, card |
| `--color-line` | `oklch(0.90 0.008 85)` / `#E4E2DB` | bordi |
| `--color-muted` | `oklch(0.55 0.005 85)` / `#6B6A64` | testi secondari |
| `--color-lime` | `oklch(0.88 0.2 125)` | **signature AI-active** |
| `--color-lime-deep` | `oklch(0.72 0.22 125)` | hover / pressed su lime |
| `--color-terra` | `oklch(0.68 0.12 45)` | accento editoriale |

**Regola d'oro sul lime**: appare solo dove c'è AI attiva (agenti che lavorano, indicatori "LIVE", cursori che lampeggiano, progress di pubblicazione). **Mai decorativo, mai su pulsanti primari generici.**

### Mapping ai token shadcn/ui esistenti

Per non rompere i componenti shadcn, sovrascrivi i token che già usano:

```
--color-background    → var(--color-paper)
--color-foreground    → var(--color-ink)
--color-card          → var(--color-paper)
--color-primary       → var(--color-ink)
--color-secondary     → var(--color-paper-2)
--color-muted         → var(--color-paper-2)
--color-accent        → var(--color-lime)        /* attenzione: solo se serve */
--color-border        → var(--color-line)
--color-ring          → var(--color-ink)
```

Dark mode: invertire `paper` e `ink`; il lime resta identico.

---

## 🔤 Sistema tipografico

- **Instrument Serif** — voce del brand, headline, quote. Variabile CSS: `--font-serif`
- **Geist** — UI, body, pulsanti. Variabile CSS: `--font-sans` (già in uso)
- **JetBrains Mono** — meta, stati, codice, timestamp. Variabile CSS: `--font-mono`

Sostituisci `Geist_Mono` → `JetBrains_Mono` e aggiungi `Instrument_Serif`.

Scala suggerita (Tailwind class → uso):
- `text-5xl/6xl/7xl` + `font-serif` → hero, titoli marketing
- `text-2xl/3xl` + `font-serif` → titoli sezione dashboard
- `text-base/sm` + `font-sans` → body, UI
- `text-xs` + `font-mono uppercase tracking-wider` → label, status, timestamp

---

## 🏷️ Wordmark

Sempre **"weposte"** (roman) + **"ai.com"** (italic, muted). Mai "WePostAI" tutto attaccato in testo pubblico — solo come identifier interno (classi, id, types).

Usa il componente `<Logo />` ovunque. Non hardcodare lo span in altre pagine.

```tsx
<Logo variant="primary" size="md" />   // default nav
<Logo variant="inverted" size="md" />  // su fondi scuri
<Logo variant="mark" size="sm" />      // favicon-size
<Logo variant="monogram" size="md" />  // app icon, watermark
```

---

## 📝 Checklist di esecuzione per Claude Code

Esegui in quest'ordine, un commit per step:

- [ ] **1.** Installa i font: nessun pacchetto nuovo (tutti via `next/font/google`)
- [ ] **2.** Sovrascrivi `src/app/globals.css` con `tokens.css`
- [ ] **3.** Aggiorna `src/app/layout.tsx` con `layout.snippet.tsx`
- [ ] **4.** Copia `icon.svg` in `src/app/icon.svg`
- [ ] **5.** Crea `public/brand/` e copia i due SVG
- [ ] **6.** Crea `src/components/shared/Logo.tsx` e `BrandMark.tsx`
- [ ] **7.** Trova e sostituisci tutti i punti dove appare "WePostAI" come stringa visibile → `<Logo />` (o stringa `weposteai.com` se serve testo)
- [ ] **8.** Aggiorna `metadata.title` in `layout.tsx` → `"weposteai.com — AI marketing agency"`
- [ ] **9.** Aggiorna `metadata.description` se serve
- [ ] **10.** `npm run build` → verifica nessun errore di tipi/import
- [ ] **11.** Avvia `npm run dev`, apri ogni route principale, verifica visivamente

**Non toccare nulla al di fuori di questa checklist.** Se una UI richiede refactor più profondo, apri un PR separato, non mescolarlo col rebrand.

---

## 🧭 Principi visivi da applicare "in giro" (se ti viene chiesto)

1. **Quasi monocromo** — 90% del brand in bianco/nero, il lime appare solo dove c'è azione AI viva
2. **Contrasto editoriale** — serif elegante + mono tech: l'agenzia è il serif, gli agenti sono il mono
3. **Niente gradiente AI-viola-blu** — mai. Nemmeno sottile.
4. **Bordi sottili, radius piccoli** — `1px` border, `rounded-sm` / `rounded-md` max
5. **Density alta ma aerata** — la dashboard deve sembrare un editor, non un cruscotto pieno di card colorate
6. **Tono italiano, diretto** — "3 post, 0 notifiche" non "Unlock your content potential"
