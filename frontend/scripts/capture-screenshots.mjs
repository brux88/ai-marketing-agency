import puppeteer from "puppeteer";
import { mkdir } from "fs/promises";
import { join } from "path";

const BASE = "http://localhost:3000";
const OUT = join(import.meta.dirname, "..", "public", "screenshots");

const PAGES = [
  { name: "01-landing-hero", url: "/", scroll: 0 },
  { name: "02-login", url: "/login", scroll: 0 },
];

const LOGGED_IN_PAGES = [
  { name: "03-dashboard", url: "/dashboard", scroll: 0 },
  { name: "04-agency-overview", url: null, scroll: 0, clickSelector: "[data-testid='agency-card'], .cursor-pointer" },
  { name: "05-project-panoramica", url: null, scroll: 0, clickSelector: "[data-testid='project-card'], .cursor-pointer" },
];

async function run() {
  await mkdir(OUT, { recursive: true });

  const browser = await puppeteer.launch({
    headless: true,
    defaultViewport: { width: 1400, height: 900 },
    args: ["--no-sandbox"],
  });

  const page = await browser.newPage();

  // Landing page screenshot
  await page.goto(`${BASE}/`, { waitUntil: "networkidle2", timeout: 15000 });
  await page.screenshot({ path: join(OUT, "01-landing-hero.png") });
  console.log("captured 01-landing-hero");

  // Login page
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle2", timeout: 15000 });
  await page.screenshot({ path: join(OUT, "02-login.png") });
  console.log("captured 02-login");

  // Login
  await page.type('input[type="email"]', "test@test.com");
  await page.type('input[type="password"]', "Test1234!");
  await page.click('button[type="submit"]');
  await page.waitForNavigation({ waitUntil: "networkidle2", timeout: 15000 });
  await new Promise(r => setTimeout(r, 2000));

  // Dashboard
  await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle2", timeout: 15000 });
  await new Promise(r => setTimeout(r, 1500));
  await page.screenshot({ path: join(OUT, "03-dashboard.png") });
  console.log("captured 03-dashboard");

  // Get agency link from dashboard
  const agencyLink = await page.evaluate(() => {
    const links = document.querySelectorAll("a[href*='/agencies/']");
    return links[0]?.getAttribute("href") || null;
  });

  if (agencyLink) {
    // Agency overview
    await page.goto(`${BASE}${agencyLink}`, { waitUntil: "networkidle2", timeout: 15000 });
    await new Promise(r => setTimeout(r, 1500));
    await page.screenshot({ path: join(OUT, "04-agency-overview.png") });
    console.log("captured 04-agency-overview");

    // Get project link
    const projectLink = await page.evaluate(() => {
      const links = document.querySelectorAll("a[href*='/projects/']");
      return links[0]?.getAttribute("href") || null;
    });

    if (projectLink) {
      // Project panoramica
      await page.goto(`${BASE}${projectLink}`, { waitUntil: "networkidle2", timeout: 15000 });
      await new Promise(r => setTimeout(r, 1500));
      await page.screenshot({ path: join(OUT, "05-project-panoramica.png") });
      console.log("captured 05-project-panoramica");

      // Contenuti tab
      const tabBase = `${BASE}${projectLink}`;
      await page.evaluate(() => {
        const tabs = document.querySelectorAll("button, a");
        for (const t of tabs) {
          if (t.textContent?.trim() === "Contenuti") { t.click(); break; }
        }
      });
      await new Promise(r => setTimeout(r, 2000));
      await page.screenshot({ path: join(OUT, "06-project-contenuti.png") });
      console.log("captured 06-project-contenuti");

      // Calendario tab
      await page.evaluate(() => {
        const tabs = document.querySelectorAll("button, a");
        for (const t of tabs) {
          if (t.textContent?.trim() === "Calendario") { t.click(); break; }
        }
      });
      await new Promise(r => setTimeout(r, 2000));
      await page.screenshot({ path: join(OUT, "07-project-calendario.png") });
      console.log("captured 07-project-calendario");

      // Programmazione tab
      await page.evaluate(() => {
        const tabs = document.querySelectorAll("button, a");
        for (const t of tabs) {
          if (t.textContent?.trim() === "Programmazione") { t.click(); break; }
        }
      });
      await new Promise(r => setTimeout(r, 2000));
      await page.screenshot({ path: join(OUT, "08-project-programmazione.png") });
      console.log("captured 08-project-programmazione");

      // Impostazioni tab
      await page.evaluate(() => {
        const tabs = document.querySelectorAll("button, a");
        for (const t of tabs) {
          if (t.textContent?.trim() === "Impostazioni") { t.click(); break; }
        }
      });
      await new Promise(r => setTimeout(r, 2000));
      await page.screenshot({ path: join(OUT, "09-project-impostazioni.png") });
      console.log("captured 09-project-impostazioni");
    }
  }

  // Chiavi API
  await page.goto(`${BASE}/settings/api-keys`, { waitUntil: "networkidle2", timeout: 15000 });
  await new Promise(r => setTimeout(r, 1500));
  await page.screenshot({ path: join(OUT, "10-chiavi-api.png") });
  console.log("captured 10-chiavi-api");

  // Abbonamento
  await page.goto(`${BASE}/settings/billing`, { waitUntil: "networkidle2", timeout: 15000 });
  await new Promise(r => setTimeout(r, 1500));
  await page.screenshot({ path: join(OUT, "11-abbonamento.png") });
  console.log("captured 11-abbonamento");

  // Notifiche
  await page.goto(`${BASE}/jobs`, { waitUntil: "networkidle2", timeout: 15000 });
  await new Promise(r => setTimeout(r, 1500));
  await page.screenshot({ path: join(OUT, "12-notifiche-jobs.png") });
  console.log("captured 12-notifiche-jobs");

  await browser.close();
  console.log(`\nAll screenshots saved to ${OUT}`);
}

run().catch(console.error);
