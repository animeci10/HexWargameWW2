// --- Hex Tabanlı Taktiksel Wargame Motoru (2.5D Canvas) ---

const HEX_RADIUS = 46;
const SQRT3 = Math.sqrt(3);

// 6 Yön Vektörü (Pointy-Topped Cube)
const HEX_DIRECTIONS = [
  { q: 1, r: 0 }, { q: 1, r: -1 }, { q: 0, r: -1 },
  { q: -1, r: 0 }, { q: -1, r: 1 }, { q: 0, r: 1 }
];

function hexDistance(a, b) {
  return (Math.abs(a.q - b.q) + Math.abs(a.r - b.r) + Math.abs((-a.q - a.r) - (-b.q - b.r))) / 2;
}

function hexToPixel(q, r, radius) {
  const x = radius * SQRT3 * (q + r / 2);
  const y = radius * 1.5 * r;
  return { x, y };
}

function pixelToHex(x, y, radius) {
  const qf = (SQRT3 / 3 * x - 1 / 3 * y) / radius;
  const rf = (2 / 3 * y) / radius;
  const sf = -qf - rf;

  let qi = Math.round(qf);
  let ri = Math.round(rf);
  let si = Math.round(sf);

  const qDiff = Math.abs(qi - qf);
  const rDiff = Math.abs(ri - rf);
  const sDiff = Math.abs(si - sf);

  if (qDiff > rDiff && qDiff > sDiff) {
    qi = -ri - si;
  } else if (rDiff > sDiff) {
    ri = -qi - si;
  }
  return { q: qi, r: ri };
}

// Birlik Şablonları Kataloğu
const UNIT_BLUEPRINTS = {
  tiger1: {
    id: "tiger1", name: "Panzer VI Tiger I", category: "Ağır Tank",
    maxStrength: 10, maxMP: 3, ammoMax: 8, fuelMax: 100,
    softAttack: 22, hardAttack: 42, penMm: 138,
    rangeMin: 1, rangeMax: 2,
    armorFront: 102, armorSide: 82, armorRear: 62,
    icon: "🐅", color: "#f59e0b"
  },
  fallschirm: {
    id: "fallschirm", name: "Fallschirmjäger", category: "Seçkin Piyade",
    maxStrength: 10, maxMP: 3, ammoMax: 8, fuelMax: 0,
    softAttack: 24, hardAttack: 14, penMm: 45,
    rangeMin: 1, rangeMax: 1,
    armorFront: 0, armorSide: 0, armorRear: 0,
    icon: "🪂", color: "#38bdf8"
  },
  flak88: {
    id: "flak88", name: "8.8 cm FlaK 36", category: "Ağır Topçu",
    maxStrength: 10, maxMP: 2, ammoMax: 8, fuelMax: 0,
    softAttack: 25, hardAttack: 46, penMm: 142,
    rangeMin: 1, rangeMax: 3,
    armorFront: 15, armorSide: 0, armorRear: 0,
    icon: "🎯", color: "#eab308"
  },
  t34_85: {
    id: "t34_85", name: "T-34/85", category: "Orta Tank",
    maxStrength: 10, maxMP: 5, ammoMax: 10, fuelMax: 120,
    softAttack: 18, hardAttack: 34, penMm: 125,
    rangeMin: 1, rangeMax: 1,
    armorFront: 90, armorSide: 45, armorRear: 45,
    icon: "🚜", color: "#ef4444"
  },
  katyusha: {
    id: "katyusha", name: "BM-13 Katyusha", category: "Roket Topçusu",
    maxStrength: 10, maxMP: 6, ammoMax: 4, fuelMax: 140,
    softAttack: 42, hardAttack: 12, penMm: 30,
    rangeMin: 2, rangeMax: 4,
    armorFront: 10, armorSide: 6, armorRear: 6,
    icon: "🚀", color: "#f87171"
  },
  guards_rifle: {
    id: "guards_rifle", name: "Kızıl Ordu Muhafızları", category: "Taarruz Piyadesi",
    maxStrength: 10, maxMP: 3, ammoMax: 10, fuelMax: 0,
    softAttack: 22, hardAttack: 10, penMm: 35,
    rangeMin: 1, rangeMax: 1,
    armorFront: 0, armorSide: 0, armorRear: 0,
    icon: "💂", color: "#f43f5e"
  }
};

// Arazi Tipleri
const TERRAIN = {
  plains: { name: "Açık Düzlük", color: "#2d4a2d", cost: 1, defBonus: 0, elev: 0 },
  forest: { name: "Orman", color: "#1b331b", cost: 2, defBonus: 0.3, elev: 0 },
  hills: { name: "Tepeler", color: "#475569", cost: 2, defBonus: 0.25, elev: 1 },
  city: { name: "Şehir / Tahkimat", color: "#64748b", cost: 1, defBonus: 0.4, elev: 0, isObjective: true }
};

class GameState {
  constructor() {
    this.map = new Map(); // key "q,r" -> Cell
    this.units = [];      // Unit list
    this.currentTurn = 1;
    this.phase = "PLAYER"; // PLAYER, AI, GAMEOVER
    this.selectedUnit = null;
    this.reachableHexes = new Set();
    this.attackableTargets = [];

    // Kamera Durumu
    this.camX = 0;
    this.camY = 0;
    this.zoom = 1.0;

    this.initMap();
    this.initUnits();
  }

  initMap() {
    const W = 13, H = 10;
    for (let r = 0; r < H; r++) {
      const rOffset = r >> 1;
      for (let q = -rOffset; q < W - rOffset; q++) {
        let type = "plains";
        if ((q + r * 2) % 6 === 0) type = "forest";
        else if ((q * 3 + r) % 8 === 0) type = "hills";
        else if (q === 5 && r === 4) type = "city";
        else if (q === 6 && r === 5) type = "city";

        this.map.set(`${q},${r}`, {
          q, r, terrain: TERRAIN[type],
          elevation: TERRAIN[type].elev
        });
      }
    }
  }

  initUnits() {
    // Oyuncu Birlikleri (Müttefikler - Mavi/Yeşil)
    this.units.push(this.createUnit("tiger1", 1, 2, 0));
    this.units.push(this.createUnit("fallschirm", 2, 3, 0));
    this.units.push(this.createUnit("flak88", 1, 4, 0));

    // AI Birlikleri (Kızıl Ordu / Mihver - Kırmızı)
    this.units.push(this.createUnit("t34_85", 8, 5, 1));
    this.units.push(this.createUnit("katyusha", 9, 6, 1));
    this.units.push(this.createUnit("guards_rifle", 7, 6, 1));
  }

  createUnit(bpKey, q, r, team) {
    const bp = UNIT_BLUEPRINTS[bpKey];
    return {
      bp,
      q, r,
      team,
      strength: bp.maxStrength,
      mp: bp.maxMP,
      ammo: bp.ammoMax,
      fuel: bp.fuelMax,
      entrenched: 0,
      hasAttacked: false,
      facing: team === 0 ? 0 : 3
    };
  }

  getUnitAt(q, r) {
    return this.units.find(u => u.q === q && u.r === r && u.strength > 0);
  }

  calculateReachable(unit) {
    const reachable = new Set();
    if (unit.mp <= 0) return reachable;

    const visited = new Map();
    const queue = [{ q: unit.q, r: unit.r, cost: 0 }];
    visited.set(`${unit.q},${unit.r}`, 0);

    while (queue.length > 0) {
      const cur = queue.shift();

      for (const dir of HEX_DIRECTIONS) {
        const nq = cur.q + dir.q;
        const nr = cur.r + dir.r;
        const key = `${nq},${nr}`;

        const cell = this.map.get(key);
        if (!cell) continue;

        const occupant = this.getUnitAt(nq, nr);
        if (occupant && occupant.team !== unit.team) continue; // Düşman üstünden geçilemez

        const stepCost = cell.terrain.cost;
        const newCost = cur.cost + stepCost;

        if (newCost <= unit.mp) {
          if (!visited.has(key) || newCost < visited.get(key)) {
            visited.set(key, newCost);
            queue.push({ q: nq, r: nr, cost: newCost });
            if (!occupant) reachable.add(key);
          }
        }
      }
    }
    return reachable;
  }

  calculateAttackTargets(unit) {
    if (unit.hasAttacked || unit.ammo <= 0) return [];
    const targets = [];
    for (const other of this.units) {
      if (other.team === unit.team || other.strength <= 0) continue;
      const d = hexDistance(unit, other);
      if (d >= unit.bp.rangeMin && d <= unit.bp.rangeMax) {
        targets.push(other);
      }
    }
    return targets;
  }

  resolveCombat(attacker, defender) {
    attacker.ammo = Math.max(0, attacker.ammo - 1);
    attacker.hasAttacked = true;

    const cell = this.map.get(`${defender.q},${defender.r}`);
    const terrainDef = cell ? cell.terrain.defBonus : 0;
    const entrenchDef = defender.entrenched * 0.15;
    const totalDef = Math.min(0.75, terrainDef + entrenchDef);

    // Vuruş Açısı (Basitleştirilmiş)
    const isArmored = defender.bp.armorFront > 0;
    let armor = defender.bp.armorFront;
    let hitAngleText = "Ön Zırh (%100)";

    // Delme Oranı
    const penRatio = armor > 0 ? (attacker.bp.penMm / armor) : 2.0;
    let dmgMult = 1.0;
    let outcome = "Etkili Vuruş";

    if (isArmored) {
      if (penRatio < 0.7) {
        dmgMult = 0.1;
        outcome = "🛡️ MERMİ SEKTİ (Sekme / Deflection)";
      } else if (penRatio < 1.0) {
        dmgMult = 0.5;
        outcome = "⚡ Kısmi Zırh Hasarı";
      } else {
        dmgMult = 1.25;
        outcome = "💥 TAM ZIRH DELME!";
      }
    }

    const basePower = isArmored ? attacker.bp.hardAttack : attacker.bp.softAttack;
    const rawDmg = (basePower * (attacker.strength / 10)) * dmgMult * (1 - totalDef) * 0.22;
    const defDamage = Math.max(dmgMult > 0.1 ? 1 : 0, Math.round(rawDmg));

    defender.strength = Math.max(0, defender.strength - defDamage);
    if (defender.entrenched > 0) defender.entrenched--;

    // Karşı Taarruz
    let counterText = "Mesafe dışı";
    let attDamage = 0;
    const dist = hexDistance(attacker, defender);
    if (defender.strength > 0 && dist >= defender.bp.rangeMin && dist <= defender.bp.rangeMax && defender.ammo > 0) {
      defender.ammo--;
      const defPower = (attacker.bp.armorFront > 0) ? defender.bp.hardAttack : defender.bp.softAttack;
      attDamage = Math.max(1, Math.round(defPower * (defender.strength / 10) * 0.14));
      attacker.strength = Math.max(0, attacker.strength - attDamage);
      counterText = `Düşman Karşı Taarruz Açtı (-${attDamage} Can)`;
    }

    return {
      outcome,
      defDamage,
      attDamage,
      counterText,
      penRatio: penRatio.toFixed(2),
      hitAngleText
    };
  }

  endTurn() {
    if (this.phase !== "PLAYER") return;
    this.phase = "AI";
    updateUI();
    showToast("🤖 Yapay Zeka Hamle Yapıyor...");

    setTimeout(() => {
      this.executeAITurn();
      this.currentTurn++;
      this.phase = "PLAYER";

      // Yeni turda birlikleri yenile
      for (const u of this.units) {
        u.mp = u.bp.maxMP;
        u.hasAttacked = false;
        if (u.ammo < u.bp.ammoMax) u.ammo = Math.min(u.bp.ammoMax, u.ammo + 2);
      }

      this.selectedUnit = null;
      this.reachableHexes.clear();
      this.attackableTargets = [];
      updateUI();
      showToast("🟢 Oyuncu Fazı Başladı (Tur " + this.currentTurn + ")");
    }, 1200);
  }

  executeAITurn() {
    const aiUnits = this.units.filter(u => u.team === 1 && u.strength > 0);
    for (const unit of aiUnits) {
      // 1. Menzilde oyuncu varsa saldır
      const targets = this.calculateAttackTargets(unit);
      if (targets.length > 0) {
        const target = targets[0];
        this.resolveCombat(unit, target);
        continue;
      }

      // 2. Oyuncuya doğru yaklaş
      const playerUnits = this.units.filter(u => u.team === 0 && u.strength > 0);
      if (playerUnits.length > 0) {
        const closest = playerUnits[0];
        const reachable = Array.from(this.calculateReachable(unit));
        if (reachable.length > 0) {
          reachable.sort((a, b) => {
            const [aq, ar] = a.split(",").map(Number);
            const [bq, br] = b.split(",").map(Number);
            return hexDistance({ q: aq, r: ar }, closest) - hexDistance({ q: bq, r: br }, closest);
          });
          const best = reachable[0].split(",").map(Number);
          unit.q = best[0];
          unit.r = best[1];
          unit.mp = 0;
        }
      }
    }
  }
}

// Canvas & UI Renderer
const canvas = document.getElementById("hexCanvas");
const ctx = canvas.getContext("2d");
const game = new GameState();

function resizeCanvas() {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight;
}
window.addEventListener("resize", resizeCanvas);
resizeCanvas();

// Başlangıç Kamera Pozisyonu (Merkez)
game.camX = canvas.width / 2 - 250;
game.camY = canvas.height / 2 - 180;

function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.save();
  ctx.translate(game.camX, game.camY);
  ctx.scale(game.zoom, game.zoom);

  // 1. Hex Hücrelerini Çiz
  for (const cell of game.map.values()) {
    const { x, y } = hexToPixel(cell.q, cell.r, HEX_RADIUS);
    const key = `${cell.q},${cell.r}`;
    const isReachable = game.reachableHexes.has(key);
    const isTarget = game.attackableTargets.some(t => t.q === cell.q && t.r === cell.r);

    drawHex(x, y - cell.elevation * 8, HEX_RADIUS, cell.terrain.color, isReachable, isTarget);

    if (cell.terrain.isObjective) {
      ctx.fillStyle = "#fbbf24";
      ctx.font = "bold 14px sans-serif";
      ctx.fillText("⭐ HEDEF", x - 26, y + 4);
    }
  }

  // 2. Birlikleri Çiz
  for (const unit of game.units) {
    if (unit.strength <= 0) continue;
    const cell = game.map.get(`${unit.q},${unit.r}`);
    const elev = cell ? cell.elevation * 8 : 0;
    const { x, y } = hexToPixel(unit.q, unit.r, HEX_RADIUS);

    drawUnit(unit, x, y - elev);
  }

  ctx.restore();
  requestAnimationFrame(draw);
}

function drawHex(x, y, radius, color, isReachable, isTarget) {
  ctx.beginPath();
  for (let i = 0; i < 6; i++) {
    const angle = (Math.PI / 3) * i + (Math.PI / 6);
    const hx = x + radius * Math.cos(angle);
    const hy = y + radius * Math.sin(angle);
    if (i === 0) ctx.moveTo(hx, hy);
    else ctx.lineTo(hx, hy);
  }
  ctx.closePath();

  ctx.fillStyle = color;
  ctx.fill();

  if (isTarget) {
    ctx.strokeStyle = "#ef4444";
    ctx.lineWidth = 3.5;
    ctx.stroke();
    // Kırmızı hedef artısı
    ctx.beginPath();
    ctx.arc(x, y, radius * 0.4, 0, Math.PI * 2);
    ctx.stroke();
  } else if (isReachable) {
    ctx.strokeStyle = "#38bdf8";
    ctx.lineWidth = 3;
    ctx.stroke();
    ctx.fillStyle = "rgba(56, 189, 248, 0.25)";
    ctx.fill();
  } else {
    ctx.strokeStyle = "rgba(255, 255, 255, 0.12)";
    ctx.lineWidth = 1;
    ctx.stroke();
  }
}

function drawUnit(unit, x, y) {
  const isSelected = game.selectedUnit === unit;

  // Takım Halkası
  ctx.beginPath();
  ctx.arc(x, y, 22, 0, Math.PI * 2);
  ctx.fillStyle = unit.team === 0 ? "#1e40af" : "#991b1b";
  ctx.fill();
  ctx.strokeStyle = isSelected ? "#38bdf8" : "#ffffff";
  ctx.lineWidth = isSelected ? 3 : 1.5;
  ctx.stroke();

  // Birlik İkonu
  ctx.font = "18px sans-serif";
  ctx.textAlign = "center";
  ctx.textBaseline = "middle";
  ctx.fillText(unit.bp.icon, x, y);

  // Can Barı
  const barW = 28, barH = 4;
  const hpPercent = unit.strength / unit.bp.maxStrength;
  ctx.fillStyle = "#334155";
  ctx.fillRect(x - barW / 2, y + 16, barW, barH);
  ctx.fillStyle = hpPercent > 0.5 ? "#10b981" : (hpPercent > 0.25 ? "#f59e0b" : "#ef4444");
  ctx.fillRect(x - barW / 2, y + 16, barW * hpPercent, barH);

  // Siper Rozeti
  if (unit.entrenched > 0) {
    ctx.fillStyle = "#38bdf8";
    ctx.font = "bold 10px sans-serif";
    ctx.fillText("🛡️" + unit.entrenched, x + 16, y - 14);
  }
}

// Dokunmatik & Fare Etkileşimleri
let isDragging = false;
let startX = 0, startY = 0;
let lastDist = 0;

canvas.addEventListener("pointerdown", e => {
  isDragging = true;
  startX = e.clientX;
  startY = e.clientY;
});

canvas.addEventListener("pointermove", e => {
  if (!isDragging) return;
  game.camX += e.movementX;
  game.camY += e.movementY;
});

window.addEventListener("pointerup", e => {
  if (Math.abs(e.clientX - startX) < 10 && Math.abs(e.clientY - startY) < 10) {
    handleTap(e.clientX, e.clientY);
  }
  isDragging = false;
});

function handleTap(screenX, screenY) {
  if (game.phase !== "PLAYER") return;

  const worldX = (screenX - game.camX) / game.zoom;
  const worldY = (screenY - game.camY) / game.zoom;
  const hex = pixelToHex(worldX, worldY, HEX_RADIUS);
  const key = `${hex.q},${hex.r}`;

  const tappedUnit = game.getUnitAt(hex.q, hex.r);

  // 1. Dost Birim Seçimi
  if (tappedUnit && tappedUnit.team === 0) {
    selectUnit(tappedUnit);
    return;
  }

  // 2. Birim Seçiliyken Hedefe Tıklama
  if (game.selectedUnit) {
    // Düşmana Tıklanmışsa Saldır
    if (tappedUnit && tappedUnit.team === 1) {
      if (game.attackableTargets.includes(tappedUnit)) {
        const report = game.resolveCombat(game.selectedUnit, tappedUnit);
        showCombatModal(game.selectedUnit, tappedUnit, report);
        selectUnit(game.selectedUnit);
        return;
      }
    }

    // Boş Alana ve Menzile Tıklanmışsa Hareket Et
    if (game.reachableHexes.has(key)) {
      game.selectedUnit.q = hex.q;
      game.selectedUnit.r = hex.r;
      game.selectedUnit.mp = 0;
      selectUnit(game.selectedUnit);
      showToast("Birlik ilerledi.");
      return;
    }
  }

  // Seçimi Kaldır
  deselect();
}

function selectUnit(unit) {
  game.selectedUnit = unit;
  game.reachableHexes = game.calculateReachable(unit);
  game.attackableTargets = game.calculateAttackTargets(unit);
  updateUI();
}

function deselect() {
  game.selectedUnit = null;
  game.reachableHexes.clear();
  game.attackableTargets = [];
  updateUI();
}

function updateUI() {
  document.getElementById("turn-text").innerText = `TUR: ${game.currentTurn} / 25`;
  const phaseEl = document.getElementById("phase-banner");
  phaseEl.innerText = game.phase === "PLAYER" ? "OYUNCU FAZI" : "YAPAY ZEKA FAZI";
  phaseEl.style.color = game.phase === "PLAYER" ? "#10b981" : "#ef4444";

  const card = document.getElementById("unit-card");
  const digBtn = document.getElementById("btn-digin");

  if (game.selectedUnit) {
    card.classList.remove("empty");
    const u = game.selectedUnit;
    document.getElementById("unit-name").innerText = u.bp.name;
    document.getElementById("unit-category").innerText = u.bp.category;
    document.getElementById("unit-hp").innerText = `${u.strength}/${u.bp.maxStrength}`;
    document.getElementById("unit-mp").innerText = u.mp;
    document.getElementById("unit-ammo").innerText = u.ammo;
    document.getElementById("unit-fuel").innerText = u.fuel;
    document.getElementById("unit-pen").innerText = `${u.bp.penMm} mm`;
    document.getElementById("unit-armor").innerText = `${u.bp.armorFront} mm`;

    digBtn.disabled = u.team !== 0 || u.mp <= 0 || u.entrenched >= 3;
  } else {
    card.classList.add("empty");
    document.getElementById("unit-name").innerText = "Bir Birlik Seçin";
    document.getElementById("unit-category").innerText = "-";
    document.getElementById("unit-hp").innerText = "--/10";
    document.getElementById("unit-mp").innerText = "--";
    document.getElementById("unit-ammo").innerText = "--";
    document.getElementById("unit-fuel").innerText = "--";
    document.getElementById("unit-pen").innerText = "-- mm";
    document.getElementById("unit-armor").innerText = "-- mm";
    digBtn.disabled = true;
  }
}

// Buton Etkinlikleri
document.getElementById("btn-endturn").addEventListener("click", () => game.endTurn());
document.getElementById("btn-digin").addEventListener("click", () => {
  if (game.selectedUnit && game.selectedUnit.mp > 0) {
    game.selectedUnit.entrenched++;
    game.selectedUnit.mp = 0;
    selectUnit(game.selectedUnit);
    showToast("🛡️ Siper derinleştirildi (+%15 Savunma)");
  }
});

document.getElementById("zoom-in").addEventListener("click", () => {
  game.zoom = Math.min(2.0, game.zoom + 0.15);
});
document.getElementById("zoom-out").addEventListener("click", () => {
  game.zoom = Math.max(0.6, game.zoom - 0.15);
});
document.getElementById("center-cam").addEventListener("click", () => {
  game.camX = canvas.width / 2 - 250;
  game.camY = canvas.height / 2 - 180;
  game.zoom = 1.0;
});

function showCombatModal(att, def, report) {
  const modal = document.getElementById("combat-modal");
  const details = document.getElementById("combat-details");
  details.innerHTML = `
    <strong>Saldıran:</strong> ${att.bp.name}<br>
    <strong>Hedef:</strong> ${def.bp.name} (${report.hitAngleText})<br>
    <strong>Balistik:</strong> Delme Oranı: ${report.penRatio}x (${report.outcome})<br>
    <strong>Verilen Hasar:</strong> -${report.defDamage} Can<br>
    <strong>Savunma Cevabı:</strong> ${report.counterText}
  `;
  modal.classList.remove("hidden");
}

document.getElementById("modal-close").addEventListener("click", () => {
  document.getElementById("combat-modal").classList.add("hidden");
});

function showToast(msg) {
  const t = document.getElementById("toast");
  t.innerText = msg;
  t.classList.remove("hidden");
  setTimeout(() => t.classList.add("hidden"), 2200);
}

// Başlat
draw();
updateUI();
