// Extracts Peru admin hierarchy (region/province/district) from OSM admin geojsonseq.
// Input: backend/osm-data/admin.geojsonseq (osmium tags-filter + osmium export)
// Output: backend/stops/Infrastructure/Seeding/geo-data.json
//   [{ CODIGO:"150101", NOMBDEP:"LIMA", NOMBPROV:"LIMA", NOMBDIST:"LIMA" }, ...]

import fs from 'node:fs'
import path from 'node:path'
import readline from 'node:readline'
import centroid from '@turf/centroid'
import pointInPolygon from '@turf/boolean-point-in-polygon'
import { feature as turfFeature, point as turfPoint } from '@turf/helpers'

const root = path.resolve(import.meta.dirname, '..')
const inFile = path.join(root, 'osm-data', 'admin-poly.geojsonseq')
const outFile = path.join(root, 'stops', 'Infrastructure', 'Seeding', 'geo-data.json')

if (!fs.existsSync(inFile)) {
  console.error(`Missing ${inFile}. Run osmium first (see README).`)
  process.exit(1)
}

const regions = []   // {name, geom}
const provinces = [] // {name, geom}
const districts = [] // {name, geom}

const stream = fs.createReadStream(inFile)
const rl = readline.createInterface({ input: stream, crlfDelay: Infinity })

let read = 0
for await (const line of rl) {
  if (!line.trim()) continue
  read++
  let f
  try { f = JSON.parse(line) } catch { continue }
  const p = f.properties || {}
  if (p.boundary !== 'administrative') continue
  if (!f.geometry) continue
  const g = f.geometry.type
  if (g !== 'Polygon' && g !== 'MultiPolygon') continue
  const name = p['name:es'] || p.name
  if (!name) continue
  const lvl = String(p.admin_level)
  const item = { name: name.trim(), geom: f.geometry, ubigeo: p['ref:INEI'] || p['ref:ubigeo'] || p.ref || null }
  if (lvl === '4') regions.push(item)
  else if (lvl === '6') provinces.push(item)
  else if (lvl === '8') districts.push(item)
}

console.log(`Read ${read} features. raw: ${regions.length} regions, ${provinces.length} provinces, ${districts.length} districts`)

// Dedupe by name (OSM may have multiple variants — keep first with valid geom)
function dedupe(list) {
  const seen = new Map()
  for (const it of list) if (!seen.has(it.name)) seen.set(it.name, it)
  return [...seen.values()]
}
const R = dedupe(regions)
const P = dedupe(provinces)
const D = dedupe(districts)
console.log(`After dedupe: ${R.length} regions, ${P.length} provinces, ${D.length} districts`)

// Build spatial parent lookup
function findParent(child, parents) {
  const c = centroid(turfFeature(child.geom))
  for (const par of parents) {
    if (pointInPolygon(c, turfFeature(par.geom))) return par
  }
  return null
}

// Assign synthetic UBIGEO-shaped codes (2/4/6 digits) to keep backend parser happy
const regionCode = new Map() // name -> "01"
const provinceCode = new Map() // name -> "0101"
const items = []

R.sort((a, b) => a.name.localeCompare(b.name))
R.forEach((r, i) => regionCode.set(r.name, String(i + 1).padStart(2, '0')))

// For each province find its region by centroid; assign code regionCode + seq within region
const provincesByRegion = new Map()
for (const prov of P) {
  const par = findParent(prov, R)
  if (!par) continue
  const arr = provincesByRegion.get(par.name) || []
  arr.push({ ...prov, region: par.name })
  provincesByRegion.set(par.name, arr)
}
for (const [region, arr] of provincesByRegion) {
  arr.sort((a, b) => a.name.localeCompare(b.name))
  arr.forEach((p, i) => provinceCode.set(p.name + '|' + region, regionCode.get(region) + String(i + 1).padStart(2, '0')))
}

// For each district find its province by centroid
const districtsByProvince = new Map()
let orphans = 0
for (const dist of D) {
  let par = findParent(dist, [...provincesByRegion.values()].flat())
  if (!par) { orphans++; continue }
  const key = par.name + '|' + par.region
  const arr = districtsByProvince.get(key) || []
  arr.push({ ...dist, province: par.name, region: par.region })
  districtsByProvince.set(key, arr)
}
console.log(`Districts orphans (no province match): ${orphans}`)

for (const [pkey, arr] of districtsByProvince) {
  arr.sort((a, b) => a.name.localeCompare(b.name))
  arr.forEach((d, i) => {
    const codigo = provinceCode.get(pkey) + String(i + 1).padStart(2, '0')
    items.push({
      CODIGO: codigo,
      NOMBDEP: d.region.toUpperCase(),
      NOMBPROV: d.province.toUpperCase(),
      NOMBDIST: d.name.toUpperCase()
    })
  })
}

console.log(`Output items: ${items.length}`)
fs.mkdirSync(path.dirname(outFile), { recursive: true })
fs.writeFileSync(outFile, JSON.stringify(items, null, 2))
console.log(`Written -> ${outFile}`)
