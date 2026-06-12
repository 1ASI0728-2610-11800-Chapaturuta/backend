// Builds a complete geo-data.json (Region/Province/District) from the INEI 2016
// UBIGEO dataset (ernestorivero/Ubigeo-Peru). Output shape matches GeoResponseDto
// consumed by GeoImportService / GeographicDataSeeder:
//   { NOMBDEP, NOMBPROV, NOMBDIST, CODIGO }  // CODIGO = 6-digit ubigeo "DDPPDD"
//
// Run:  node scripts/build-geo-from-ubigeo.mjs
//
// Replaces the previous OSM-derived snapshot which was missing Callao (region 07)
// and ~180 districts, causing gaps in district/department autocomplete.

import { writeFileSync, mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const BASE = 'https://raw.githubusercontent.com/ernestorivero/Ubigeo-Peru/master/json';
const OUT = join(dirname(fileURLToPath(import.meta.url)), '..', 'stops', 'Infrastructure', 'Seeding', 'geo-data.json');

async function getJson(name) {
  const res = await fetch(`${BASE}/${name}`);
  if (!res.ok) throw new Error(`HTTP ${res.status} for ${name}`);
  return res.json();
}

const up = (s) => String(s ?? '').trim().toUpperCase();

const [departamentos, provincias, distritos] = await Promise.all([
  getJson('ubigeo_peru_2016_departamentos.json'),
  getJson('ubigeo_peru_2016_provincias.json'),
  getJson('ubigeo_peru_2016_distritos.json'),
]);

const depName = new Map(departamentos.map((d) => [d.id, up(d.name)]));
const provName = new Map(provincias.map((p) => [p.id, up(p.name)]));

const records = distritos.map((d) => ({
  NOMBDEP: depName.get(d.department_id) ?? '',
  NOMBPROV: provName.get(d.province_id) ?? '',
  NOMBDIST: up(d.name),
  CODIGO: d.id, // already 6-digit "DDPPDD"
}));

// Sanity checks
const bad = records.filter((r) => !/^[0-9]{6}$/.test(r.CODIGO) || !r.NOMBDEP || !r.NOMBPROV || !r.NOMBDIST);
if (bad.length) {
  console.error(`WARNING: ${bad.length} malformed records, e.g.`, bad[0]);
}
const regions = new Set(records.map((r) => r.CODIGO.slice(0, 2)));
const provs = new Set(records.map((r) => r.CODIGO.slice(0, 4)));

mkdirSync(dirname(OUT), { recursive: true });
writeFileSync(OUT, JSON.stringify(records, null, 0), 'utf8');

console.log(`Wrote ${records.length} districts -> ${OUT}`);
console.log(`Regions: ${regions.size}, Provinces: ${provs.size}, Districts: ${records.length}`);
