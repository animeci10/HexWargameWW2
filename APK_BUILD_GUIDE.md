# 📱 HexWargameWW2 — Android APK Derleme Rehberi

Bu belge, oyununuzun Unity C# çekirdeğinden nasıl resmi bir **Android APK** dosyası elde edeceğinizi açıklar.

---

## ⚠️ Önemli Teknik Bilgi: Unity Neden Doğrudan Telefonda Derlenemez?

Unity oyunları basit bir Java/HTML uygulaması değildir:
1. Yazdığımız C# kodları, Unity'nin **IL2CPP (Intermediate Language to C++)** derleyicisi tarafından doğrudan makine koduna (ARM64 binary) çevrilir.
2. Unity'nin tescilli grafik motoru (`libunity.so`, `libmain.so`), HLSL/SPIR-V gölgelendirici derleyicisi ve sahne serileştiricisi yalnızca **Windows, macOS ve 64-bit Linux** üzerinde çalışan **Unity Editor** yazılımına aittir.
3. Dünyada Android telefon içinde doğrudan çalışan bir Unity Editor bulunmadığı için derleme işlemi ya **Masaüstü Unity Hub** ile ya da **Ücretsiz Bulut Sunucuları (CI/CD)** ile yapılır.

---

## 🚀 1. Yol: Bilgisayar Gerektirmeden Bulutta Otomatik APK Derleme (GitHub Actions)

Projeye eklediğimiz [build_apk.yml](file:///data/user/0/com.agy/files/.gemini/antigravity-app/scratch/HexWargameWW2/.github/workflows/build_apk.yml) dosyası ile GitHub sunucuları sizin yerinize APK'yı derler:

1. Bu klasörü veya zip dosyasını bir GitHub deposuna (Repository) yükleyin.
2. Deponuzun **Actions** sekmesine gidin.
3. GitHub Actions otomatik olarak resmi bir Unity Linux sunucusu başlatır, C# kodlarını IL2CPP ile derler ve **`HexWargameWW2.apk`** dosyasını çıktı olarak verir.
4. Çıkan APK linkine telefonunuzdan tıklayarak hemen yükleyebilirsiniz!

---

## 🖥️ 2. Yol: Bilgisayarda Unity Editor ile Tek Tıkla APK Çıkarma (En Hızlı Yöntem)

Projenin tüm Android yapılandırma ayarları ([ProjectSettings.asset](file:///data/user/0/com.agy/files/.gemini/antigravity-app/scratch/HexWargameWW2/ProjectSettings/ProjectSettings.asset) ve [AndroidManifest.xml](file:///data/user/0/com.agy/files/.gemini/antigravity-app/scratch/HexWargameWW2/Assets/Plugins/Android/AndroidManifest.xml)) önceden yapılmıştır.

1. **Unity Hub'ı Açın:** Unity 2022.3 LTS veya daha yeni bir sürüm kurulu olsun (Android Build Support modülü seçili olmalı).
2. **Projeyi Açın:** `HexWargameWW2` klasörünü seçin.
3. **Build Alın:**
   - Üst menüden **File -> Build Settings** seçeneğine tıklayın.
   - Platform olarak **Android**'i seçin.
   - Sağ alttaki **"Build"** butonuna tıklayın ve `HexWargameWW2.apk` olarak kaydedin.
4. Hazırlanan APK dosyasını telefonunuza atıp kurun!
