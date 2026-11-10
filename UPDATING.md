# 🔄 HexWargameWW2 — Güncelleme ve Genişletme Rehberi

Bu proje, oyunun gelecekte kolayca güncellenebilmesi, modlanabilmesi ve genişletilebilmesi için **Modüler & Veri Odaklı (Data-Driven)** olarak tasarlanmıştır.

---

## 1. Kod Derlemeden Yeni Birim Ekleme (JSON Üzerinden)

Oyun, tüm birim istatistiklerini `Assets/Resources/Data/Units/ww2_units_catalog.json` dosyasından okuyabilir.

### Yeni Bir Tank veya Piyade Eklemek:
Tek yapmanız gereken JSON dizisine yeni bir obje eklemektir:

```json
{
  "unitId": "ger_king_tiger",
  "displayName": "Tiger II (Königstiger)",
  "faction": "Germany",
  "historicalYear": 1944,
  "category": "HeavyTank",
  "targetClass": "HeavyArmor",
  "description": "8.8 cm KwK 43 L/71 topu ve 150mm eğimli ön zırhı ile dönemin en ölümcül tankı.",
  "maxMP": 3,
  "movementType": "HeavyTracked",
  "fuelCapacity": 90,
  "fuelPerHex": 14,
  "softAttack": 26,
  "hardAttack": 48,
  "penetrationMm": 175,
  "attackRangeMin": 1,
  "attackRangeMax": 2,
  "suppressionPower": 18,
  "ammoCapacity": 8,
  "armorFrontMm": 150,
  "armorSideMm": 80,
  "armorRearMm": 80,
  "maxStrength": 10
}
```
Oyun bir sonraki açılışında `UnitDataLoader.LoadUnitsFromJson()` metodu bu birimi otomatik olarak oyuna dahil eder.

---

## 2. Dengeleme (Balance Patch) Güncellemeleri

- Birliklerin zırh delme değerleri (`penetrationMm`), ön/yan zırh kalınlıkları (`armorFrontMm`), veya yakıt tüketimleri değiştiğinde **C# kodlarına dokunmanıza gerek yoktur**.
- Yalnızca `ww2_units_catalog.json` dosyasını güncelleyebilir veya bir sunucudan uzaktan indirterek (OTA Update) oyunculara yeni APK çıkartmadan denge yamaları gönderebilirsiniz.

---

## 3. Unity Package Manager (UPM) ile Güncelleme

Proje kök dizininde `package.json` yer aldığı için bu çekirdek motoru Unity projelerine doğrudan bir paket olarak bağlayabilirsiniz:

1. **Yerel Klasör Bağlama (Local Disk):**
   - Unity'de **Window -> Package Manager** açın.
   - Sol üstteki `+` butonuna basıp **"Add package from disk..."** seçeneğini seçin.
   - Bu dizindeki `package.json` dosyasını seçin.
   - Bu sayede bu çekirdek motorda yapılan her değişiklik bağlı tüm Unity projelerinde anında güncellenir!

2. **Git Deposu ile Dağıtım:**
   - Bu klasörü bir Git deposuna (GitHub/GitLab) yüklediğinizde, Package Manager'a `https://github.com/kullanici/hex-wargame-ww2.git` linkini ekleyerek tek tıkla güncelleme alabilirsiniz.

---

## 4. Yeni Arazi Türleri Ekleme

Yeni araziler (örneğin *Bocage*, *Donmuş Nehir*, *Kum Fırtınası*):
1. Unity Editörde **Sağ Tık -> Create -> Hex Wargame -> Terrain Data** seçeneğiyle yeni bir `TerrainDataSO` asseti oluşturun.
2. Piyade, tekerlekli ve zırhlı araçlar için MP maliyetini ve savunma çarpanını tanımlayın.
