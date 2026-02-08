# LLM Kullanım Dokümantasyonu

> Bu dosyayı case boyunca kullandığınız LLM (ChatGPT, Claude, Copilot vb.) etkileşimlerini belgelemek için kullanın.
> Dürüst ve detaylı dokümantasyon beklenmektedir.

## Özet

| Bilgi | Değer |
|-------|-------|
| Toplam prompt sayısı | X |
| Kullanılan araçlar | ChatGPT / Claude / Copilot |
| En çok yardım alınan konular | [liste] |
| Tahmini LLM ile kazanılan süre | X saat |

---

## Prompt 1: Refactoring LuduArts Interaction System

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-07 22:30]

**Prompt:**
```
Hey Claude! Hope u are okay!

Today, I need help about refactoring one of my technical cases; LuduArts' Interaction System.

We've create and improve this project with one of your instances but it didn't go well as I think.

So, with the unity skills that we recently created, I will ask steps when I need help and want you to show me the way or fix/create a class if needed.

We can start if u ready!
```

**Alınan Cevap (Özet):**
```
Hey Egemen! I'm doing great, thanks! 😊
.
.
I like your approach of going step-by-step. This way we can tackle each issue methodically and make sure the refactoring goes smoothly this time around.
Whenever you're ready, just share:

What specific part of the interaction system you want to start with
What issues you're facing or what didn't work well from the previous attempt
Any relevant code snippets or classes that need attention

Let's make this interaction system solid! 🎮
```

**Nasıl Kullandım:**
- [x] Direkt kullandım (değişiklik yapmadan)
- [ ] Adapte ettim (değişiklikler yaparak)
- [ ] Reddettim (kullanmadım)

**Açıklama:**
> Bu projeyi refactor etmeye başlarken bir rehber + yardımcı ile başlamak istedim ve 
> işin en başından onu da dahil etmek istedim

---

## Prompt 2: [Check interactable class modifications]

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** 2026-02-08 00:08

**Prompt:**
```
[4 .cs file uploaded]

I am trying to change some core features about interactable classes.

In old system, interactables don't have any idea about who is interactor; but now, they know who is interacting (if the implementation is correct :)) 

I cannot be sure about those classes are correct and system is working perfect. So, maybe u wanna check and show me what could possibly go wrong and what could be improve.

Take your time, I will be waiting for you analysis!
```

**Alınan Cevap (Özet):**
```
8 maddeden oluşan bir sorun tespiti ve düzeltme önerileri ile cevap verdi.
```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Anlattığım sorunu oldukça iyi anlamıştı ve Claude'a eklediğim skill sayesinde 
> tek seferde verdiği cevabı minimal düzeltme ile işleme alabildim.

---

## Prompt 3: Class check-up and bug-fix

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** 2026-02-08 13:02

**Prompt:**
```
[4 .cs file uploaded]

Thanks for the last help, It works!

Now, I need more basic thing, I need to handle that door script for making animations or transform manipulations correctly.

I tried some ways to rotate door around door hinge but current situation is a little broken, idk... maybe I need some sleep...

Can u analyze firstly door class, then the other item classes of mine? What are u thinking about them?
```

**Alınan Cevap (Özet):**
```
Door class'ında kritik sorunlar olduğunu, çözümlerini ve diğer classların analizlerini iletti.
```

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunların çözüm yollarını, hataların kaynaklarını iyi bir şekilde gösterdiği için
> hızlanmama ve temiz ilerlememe oldukça yardımcı oldu. Verdiği çıktıları kendime uyacak şekilde değiştirerek 
> spesifik sorunları çözdüm.

---

## Genel Değerlendirme

### LLM'in En Çok Yardımcı Olduğu Alanlar
1. [Alan 1]
2. [Alan 2]
3. [Alan 3]

### LLM'in Yetersiz Kaldığı Alanlar
1. [Alan 1 - neden yetersiz kaldığı]
2. [Alan 2]

### LLM Kullanımı Hakkında Düşüncelerim
> [Bu case boyunca LLM kullanarak neler öğrendiniz?
> LLM'siz ne kadar sürede bitirebilirdiniz?
> Gelecekte LLM'i nasıl daha etkili kullanabilirsiniz?]

---

## Notlar

- Her önemli LLM etkileşimini kaydedin
- Copy-paste değil, anlayarak kullandığınızı gösterin
- LLM'in hatalı cevap verdiği durumları da belirtin
- Dürüst olun - LLM kullanımı teşvik edilmektedir

---

*Bu şablon Ludu Arts Unity Intern Case için hazırlanmıştır.*
