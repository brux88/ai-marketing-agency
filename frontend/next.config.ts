import type { NextConfig } from "next";
import { execSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { join } from "node:path";

const pkg = JSON.parse(
  readFileSync(join(__dirname, "package.json"), "utf8"),
) as { version: string };

let commitSha = "";
try {
  commitSha = execSync("git rev-parse --short HEAD", { encoding: "utf8" })
    .trim();
} catch {
  commitSha = process.env.GITHUB_SHA?.slice(0, 7) ?? "";
}

const nextConfig: NextConfig = {
  output: "standalone",
  env: {
    NEXT_PUBLIC_APP_VERSION: pkg.version,
    NEXT_PUBLIC_APP_COMMIT: commitSha,
    NEXT_PUBLIC_APP_BUILD_DATE: new Date().toISOString().slice(0, 10),
  },
};

export default nextConfig;
