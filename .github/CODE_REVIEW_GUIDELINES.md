# 🛠️ Code Review Guidelines

## 📌 1. Doel van Code Reviews  
Het doel van code reviews is om:  
- De codekwaliteit te verbeteren.  
- Bugs en fouten vroegtijdig te detecteren.  
- Best practices en consistente stijlrichtlijnen te handhaven.  
- Kennisdeling binnen het team te bevorderen.  

---

## 🔄 2. Code Review Workflow  
1. **Taak afronden**  
   - Schrijf schone, goed gedocumenteerde code.  
   - Voeg duidelijke comments toe waar nodig.  
   - Test de code voordat je een **Pull Request (PR)** aanmaakt.  

2. **Pull Request (PR) aanmaken**  
   - Zorg dat de PR een duidelijke beschrijving heeft. Zie hiervoor de [Pull Request Template](./PULL_REQUEST_TEMPLATE.md).  
   - Link de PR naar de juiste Trello-taak.  
   - Wijs minimaal één reviewer aan (niet jezelf!).  

3. **Reviewproces**  
   - Een ander teamlid dan de auteur beoordeelt de code.  
   - De reviewer checkt de code op de **Checklist** (zie hieronder).  
   - Feedback wordt professioneel en constructief gegeven.  
   - De ontwikkelaar verwerkt de feedback en dient de PR opnieuw in.  

4. **Merge & Afsluiting**  
   - Na goedkeuring door een reviewer kan de PR worden gemerged.  
   - De Trello-taak wordt verplaatst naar **"Klaar"**.  
   - De reviewer of een andere teamgenoot merged de PR (niet de auteur!).  

---

## ✅ 3. Code Review Checklist  
### 🔹 Algemeen  
- [ ] Code werkt zoals verwacht en lost het juiste probleem op.  
- [ ] Code is goed leesbaar en begrijpelijk voor anderen.  
- [ ] Er zijn geen onnodige wijzigingen of tijdelijke console.log/debug statements.  

### 🔹 Code Kwaliteit & Stijl  
- [ ] Variabelen en functies hebben logische, beschrijvende namen.  
- [ ] Code is modulair en herbruikbaar waar mogelijk.  
- [ ] Geen onnodige duplicatie van code.  
- [ ] Indentatie en formatting zijn consistent (gebruik bijv. Prettier of een linter).  

### 🔹 Tests & Debugging  
- [ ] Alle relevante tests zijn succesvol uitgevoerd.  
- [ ] Er is testdekking voor nieuwe of gewijzigde functionaliteiten.  
- [ ] Er zijn geen duidelijke bugs of fouten in de logica.  

### 🔹 Performance & Security  
- [ ] Geen onnodige loops of zware bewerkingen die de prestaties beïnvloeden.  
- [ ] Geen gevoelige gegevens hardcoded in de code.  
- [ ] Inputvalidatie is correct geïmplementeerd.  

---

## 🔍 4. Feedback geven als Reviewer  
🔹 **Constructief en specifiek** → "Deze functie kan efficiënter door X te gebruiken."  
🔹 **Professioneel en objectief** → Vermijd persoonlijke opmerkingen.  
🔹 **Duidelijke actiepunten** → Wat moet aangepast worden en waarom?  

**Voorbeeld:**  
❌ *"Dit is slechte code."*  
✅ *"Deze functie kan eenvoudiger door een ingebouwde functie te gebruiken, zoals map() in plaats van een for-loop."*  

---

📌 **Bekijk ook de [Pull Request Template](./PULL_REQUEST_TEMPLATE.md) voor meer details over hoe je een PR correct aanmaakt.**
