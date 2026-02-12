# LLM Kullanım Dokümantasyonu

> Bu dosyayı case boyunca kullandığınız LLM (ChatGPT, Claude, Copilot vb.) etkileşimlerini belgelemek için kullanın.
> Dürüst ve detaylı dokümantasyon beklenmektedir.

## Özet

| Bilgi | Değer                                                                              |
|-------|------------------------------------------------------------------------------------|
| Toplam prompt sayısı | 10                                                                                 |
| Kullanılan araçlar | Claude                                                                             |
| En çok yardım alınan konular | Sistem Mimarisi<br/>Mentörlük<br/>Class geliştirme<br/>Kod Kalitesi<br/>Bug Çözümü |
| Tahmini LLM ile kazanılan süre | 4-5 saat                                                                           |

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
**Tarih/Saat:** [2026-02-08 00:08]

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
**Tarih/Saat:** [2026-02-08 13:02]

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

## Prompt 4: Switch.cs improvement

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 17:18]

**Prompt:**
```
Aaallright then, let's do some magic on Switch.cs!

It should open chest or any objects interact!
Maybe I can change skybox lightning with that switch! 

I want to hold possibilities in my hand in general.

Let's work!

```

**Alınan Cevap (Özet):**
```
Gereğinden fazla gelişmiş ve yaklaşık 800+ satıra sahip bir class verdi.
```

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [ ] Adapte ettim
- [x] Reddettim

**Açıklama:**
> Çalıştığım proje için çok gereksiz bir script olduğunu ve neye ihtiyacım olduğunu anlatarak sonrasında daha 
> optimize ve ihtiyaçlarımı karşılayan bir class elde ettim.

---

## Prompt 5: Improving Environment

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 19:42]

**Prompt:**
```
I need to set environment for that system now! 
How can I set environment look professional? 
I found https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526 
this on unity asset store, should I use it?

```

**Alınan Cevap (Özet):**
```
GREAT QUESTION, EGEMEN! But let me give you some critical advice for technical cases!
*
*
💡 THE GOLDEN RULE

"If they remember your environment more than your code, you failed."

Your interaction system should be the STAR, not the pretty walls! ⭐

🎯 MY FINAL ADVICE
DO THIS:

Use ProBuilder (30 mins)
Simple materials (solid colors)
Basic lighting (1-2 lights)
Focus on layout (show interactions clearly)
Spend remaining time on CODE

```

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [x] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Bir teknik case için görsele gereğinden fazla zaman harcamamam gerektiğini ve ProBuilder paketi ile basit
> bir environment tasarlayabileceğimi gösterdi.

---

## Prompt 6: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:27]

**Prompt:**
```
Whenever I pickup a key, whatever key it is, my UI shows it as a golden key even it is not. 

My system works correctly, bronze key only opens bronze door so gold key is just an UI bug.

Can u help me to fix it?

```

**Alınan Cevap (Özet):**
```
AH! Classic UI bug! 🎯 The issue is likely in your PlayerKeyInventory.GetKeyData() method!
*
*

📋 UPLOAD PLAYERINVENTORY.CS
Share the file and I'll spot the bug instantly! It's definitely in the GetKeyData() method (or similar). 💪

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü için dosya yüklememi istedi

---

## Prompt 7: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:31]

**Prompt:**
```
[File uploaded]

Here u go!

```

**Alınan Cevap (Özet):**
```
FOUND IT! 🎯
The PlayerKeyInventory.cs code looks perfect! The GetKeyData() method is correct
So the bug is NOT here! The problem is in how KeyPickup is calling AddKey()!
*
*
📋 UPLOAD KEYP PICKUP.CS
Please upload your KeyPickup.cs file (or whatever script handles picking up keys) and I'll spot the bug immediately!
The issue is definitely in how you're:

Assigning SO_KeyItem in the Inspector for each key prefab
Or passing the wrong keyData to AddKey()

Let me see that file! 💪

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü için tekrar dosya yüklememi istedi

---

## Prompt 8: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:32]

**Prompt:**
```
[File uploaded]

This is the one u want!

```

**Alınan Cevap (Özet):**
```
FOUND IT! IT'S AN INSPECTOR SETUP BUG! 🎯
*
*
Check your prefabs in the Inspector and let me know what you find! I bet all of them have SO_GoldKey assigned! 🎯

```

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [ ] Adapte ettim
- [x] Reddettim

**Açıklama:**
> Sorunun inspectorda olduğunu söyledi fakat sorun orada değildi.

---

## Prompt 9: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:33]

**Prompt:**
```
[Screenshot uploaded]

Sorry but u are wrong dude...

```

**Alınan Cevap (Özet):**
```
YOU'RE ABSOLUTELY RIGHT - I WAS WRONG! 🙏
The Bronze Key setup is perfect:

Key Type: Bronze ✅
Key Item Data: SO_BronzeKey ✅

So the bug is NOT in the prefabs! My apologies!
*
*
Let me see that KeySlotUI.cs file! 💪

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü içn tekrar dosya yüklememi istedi

---

## Prompt 10: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:34]

**Prompt:**
```
[File uploaded]

```

**Alınan Cevap (Özet):**
```
AH! Now I see it! 👀
The KeySlotUI.cs code looks correct too!
*
*
Upload that SO_BronzeKey inspector screenshot and I'll confirm! 📸

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü için tekrar dosya yüklememi istedi

---

## Prompt 10: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:38]

**Prompt:**
```
[Screenshot uploaded]

```

**Alınan Cevap (Özet):**
```
THERE IT IS! FOUND THE BUG! 🎯
Look at your SO_BronzeKey:

Icon: None (Sprite) ❌

The Icon field is EMPTY!
*
*
Claude's response was interrupted

```

**Nasıl Kullandım:**
- [ ] Direkt kullandım
- [ ] Adapte ettim
- [x] Reddettim

**Açıklama:**
> Sorunun sadece görsel olduğunu zannetti fakat isim olarak da yanlış data iletildiğini anlattım

---

## Prompt 10: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:39]

**Prompt:**
```
But the problem is: my UI slot show gold color AND the name "Gold Key".

So it shouldn't be just sprite or icon thing

```

**Alınan Cevap (Özet):**
```
OH WAIT! YOU'RE RIGHT! 🎯
If the UI shows:

Gold COLOR ✨
"Gold Key" name 📝

Then it's NOT a sprite issue - the entire SO_BronzeKey data is wrong!
*
*
Upload that full SO_BRONZEKEY screenshot! 📸

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü için 1 adet screenshot istedi

---

## Prompt 10: UI Golden Key Bug

**Araç:** [Claude Sonnet 4.5 Extended]
**Tarih/Saat:** [2026-02-08 20:39]

**Prompt:**
```
But the problem is: my UI slot show gold color AND the name "Gold Key".

So it shouldn't be just sprite or icon thing

```

**Alınan Cevap (Özet):**
```
OH WAIT! YOU'RE RIGHT! 🎯
If the UI shows:

Gold COLOR ✨
"Gold Key" name 📝

Then it's NOT a sprite issue - the entire SO_BronzeKey data is wrong!
*
*
Upload that full SO_BRONZEKEY screenshot! 📸

```

**Nasıl Kullandım:**
- [x] Direkt kullandım
- [ ] Adapte ettim
- [ ] Reddettim

**Açıklama:**
> Sorunun çözümü için 1 adet screenshot istedi, fakat o screenshot'da doğruydu ve bu problemin çözümü
> benim bu case'i bitirmemi çok öteleyeceği için üzerinde durmayı bıraktım.

---

## Genel Değerlendirme

### LLM'in En Çok Yardımcı Olduğu Alanlar
1. Proje Takibi
2. Method ve class geliştirmeleri
3. Mentörlük

### LLM'in Yetersiz Kaldığı Alanlar
1. Spesifik bug çözümü - projenin tüm adımlarını incelemedeğim için LLM'i bug'ın kökenine yeter kadar yönlendiremedim
2. Claude Code ile çalışmadığım için teker teker dosya iletmem gerekti ve LLM projenin tümüne hakim olamadı.

### LLM Kullanımı Hakkında Düşüncelerim
> Bu case boyunca LLM kullanarak neler öğrendiniz? - [ProBuilder]
> LLM'siz ne kadar sürede bitirebilirdiniz? - [+4 saat]
> Gelecekte LLM'i nasıl daha etkili kullanabilirsiniz? - [Claude Code üzerinden daha aktif çalışarak]

---

## Notlar

- Her önemli LLM etkileşimini kaydedin
- Copy-paste değil, anlayarak kullandığınızı gösterin
- LLM'in hatalı cevap verdiği durumları da belirtin
- Dürüst olun - LLM kullanımı teşvik edilmektedir

---

*Bu şablon Ludu Arts Unity Intern Case için hazırlanmıştır.*
