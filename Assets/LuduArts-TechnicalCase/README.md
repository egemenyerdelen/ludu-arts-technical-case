# Interaction System – Egemen Yerdelen
> Ludu Arts Unity Developer Intern Technical Case

## Proje Bilgileri

| Bilgi | Değer      |
|------|------------|
| Unity Versiyonu | 6000.3.6f1 |
| Render Pipeline | URP        |
| Case Süresi | 12 saat    |
| Tamamlanma Oranı | ~%99       |

> Bu case, verilen süre içerisinde **interaction mimarisini doğru ve sürdürülebilir şekilde kurmaya** 
> odaklanılarak geliştirilmiştir.

---

## Kurulum

1. Repository’yi klonlayın:
```bash
git clone https://github.com/egemenyerdelen/ludu-arts-technical-case.git
```

2. Unity Hub üzerinden projeyi açın
3. `Assets/InteractionSystem/Scenes/TestScene.unity` sahnesini açın
4. Play tuşuna basın

---

## Nasıl Test Edilir

### Kontroller

| Tuş | Aksiyon |
|----|--------|
| WASD | Hareket |
| Mouse | Bakış yönü |
| E | Etkileşim |

---

### Test Senaryoları

#### 1. Door Test
- Kapıya yaklaşınca interaction prompt görünür
- `E` ile kapı açılır
- Kilitli kapılar için uygun geri bildirim gösterilir

#### 2. Key + Locked Door Test
- Kilitli kapıya yaklaşınca “Key Required” mesajı görünür
- Anahtar toplandıktan sonra kapı açılabilir

#### 3. Switch Test
- Switch aktif edildiğinde bağlı objeye event gönderilir
- Event bağlantıları Inspector üzerinden yapılabilir

#### 4. Chest Test
- Sandık etkileşiminde hold interaction başlar
- Progress bar dolduğunda sandık açılır
- Tek seferlik açılma mantığı uygulanmıştır

---

## Mimari Kararlar

### Interaction System Yapısı

```
PlayerInteractor
 ├── InteractionDetector (Raycast based)
 ├── CurrentInteractable
 ├── Interaction State (Instant / Hold / Toggle)
 └── Interaction UI Feedback
```

### Neden bu yapıyı seçtim?
- Interaction mantığını **oyuncudan ve objelerden ayrıştırmak**
- Yeni interactable’ların minimum kodla eklenebilmesini sağlamak
- UI, input ve gameplay logic’in birbirine bağımlı olmamasını hedeflemek

### Alternatifler
- Tek bir base `Interactable` sınıfı düşünüldü
- Ancak bu yaklaşımın uzun vadede rigid olacağına karar verildi

### Trade-off’lar
- Esneklik arttı
- Ancak zaman kısıtı nedeniyle tüm interaction türleri %100 detaylandırılamadı

---

## Kullanılan Design Patterns

| Pattern | Kullanım Yeri | Neden |
|------|-------------|------|
| Observer | Switch → Door | Loose coupling |
| State | Door / Chest | Açık–kapalı–kilitli durumlar |
| Strategy (kısmi) | Interaction Types | Interaction davranışlarını ayırmak |

---

## Ludu Arts Standartlarına Uyum

### C# Coding Conventions

| Kural | Uygulandı | Not |
|----|----|---|
| m_ prefix | ☑ | |
| s_ prefix | ☑ | |
| k_ prefix | ☑ | |
| Region kullanımı | ☑ | |
| XML documentation | ☑ | |
| Silent bypass yok | ☑ | |
| Explicit interface impl. | ◼ | Kısmen |

---

### Naming Convention

| Kural | Uygulandı | Örnek |
|----|----|----|
| P_ Prefab | ☑ | P_Door, P_Chest |
| M_ Material | ☑ | M_Door_Wood |
| SO naming | ☑ | SO_Key_Red |

---

### Prefab Kuralları

| Kural | Uygulandı | Not |
|----|----|---|
| Transform reset | ☑ | |
| Pivot bottom-center | ☑ | |
| Collider tercihi | ☑ | Box collider |
| Hierarchy düzeni | ☑ | |

---

### Zorlandığım Noktalar
- Ludu Arts coding & prefab standartlarını **ilk defa uygularken**
- Interaction system’i tamamlamadan önce mimariyi doğru kurma isteği
- Süreyi feature tamamlamaya değil, **temel sistemi temiz kurmaya** harcamam

---

## Tamamlanan Özellikler

### Zorunlu (Must Have)

- ☑ Core Interaction System
    - ☑ IInteractable
    - ☑ InteractionDetector
    - ☑ Range kontrolü

- ☑ Interaction Types
    - ☑ Instant
    - ☑ Hold
    - ☑ Toggle

- ☑ Interactable Objects
    - ☑ Door
    - ☑ Key Pickup
    - ☑ Switch
    - ☑ Chest

- ☑ UI Feedback
    - ☑ Prompt
    - ☑ Dynamic text
    - ☑ Hold progress
    - ☑ Fail feedback

- ☑ Simple Inventory
    - ☑ Key toplama
    - ☑ UI listesi

---

## Bilinen Limitasyonlar

### Tamamlanamayan Özellikler
1. -

### Bilinen Bug’lar
1. UI tarafında inventory slotlarının gösterimi.

### İyileştirme Önerileri
1. InteractionDefinition yapısının ScriptableObject olarak ayrılması

---

## İletişim

| Bilgi | Değer |
|----|----|
| Ad Soyad | Egemen Yerdelen |
| E-posta | egemen.yerdelen@gmail.com |
| LinkedIn | linkedin.com/in/egemenyerdelen |
| GitHub | github.com/egemenyerdelen |

---

*Bu proje Ludu Arts Unity Developer Intern Case kapsamında, verilen süre limitleri içinde hazırlanmıştır.*
