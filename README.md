# 🎖️ HexWargameWW2 — II. Dünya Savaşı 2.5D Hex Taktik Motoru

Bu proje, Android platformu için yüksek performanslı, derin askeri mekaniklere ve zengin birim çeşitliliğine sahip **2.5D Hex Tabanlı Taktiksel Wargame** çekirdek mimarisidir.

---

## 📁 Proje Dosya Yapısı

```
Assets/Scripts/
├── Core/
│   └── HexCoordinates.cs          # Küp/Eksenel matematik, mesafe, görüş hattı, 2.5D izometrik dönüşüm
├── Data/
│   ├── Enums.cs                   # Fraksiyonlar, birlik kategorileri, zırh sınıfları, vuruş açıları
│   ├── TerrainDataSO.cs           # ScriptableObject arazi verileri (maliyetler, siper, savunma)
│   ├── UnitBlueprintSO.cs         # ScriptableObject birlik şablonu (balistik, zırh, yakıt, mühimmat)
│   └── WW2UnitCatalogFactory.cs   # Tiger I, T-34/85, Sherman Easy Eight, Katyusha, Flak 88 hazır şablonları
├── Simulation/
│   ├── UnitInstance.cs            # Canlı muharebe durumu (HP, MP, Yakıt, Mühimmat, Moral, Siper Seviyesi)
│   ├── HexPathfinder.cs           # Zone of Control (ZoC) destekli Hex A* yol bulma & menzil hesaplama
│   ├── CombatEngine.cs            # Zırh delme (Penetration), vuruş açısı (Ön/Yan/Arka), bastırma ve karşı taarruz
│   └── SupplyEngine.cs            # İkmal üssü BFS yayılımı, düşman ZoC blokajı ve kuşatma (Kessel) kontrolü
└── Input/
    └── MobileHexCameraController.cs # Android dokunmatik ekran: Pan, Pinch-to-zoom ve Hex seçimi
```

---

## 🚀 Unity Entegrasyon Adımları

1. Unity Hub üzerinden yeni bir **2D** veya **3D (URP)** projesi açın (Unity 2022.3 LTS veya 6.0 önerilir).
2. Bu `HexWargameWW2/Assets/Scripts` klasörünü Unity projenizin `Assets/` dizinine kopyalayın.
3. Sahneye boş bir GameObject ekleyip üzerine `MobileHexCameraController.cs` bileşenini atayın.
4. Çekirdek mantık `Pure C#` olduğu için editörde veya testlerde Unity sahnelerini beklemeksizin anlık birim simülasyonları çalıştırılabilir.
