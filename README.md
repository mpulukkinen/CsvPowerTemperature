# Projektin nimi

Csv power to temperature: Tarkoituksena sisäänottaa Elenia Aina-portaalin tuntikohtaista sähkönkulutusdataa, määrittellä vertailukohta ja näyttää lämpötilakohtaisesti, paljonko sähköä on kulunut ajanjaksolla / vertailujaksoilla. 
Koska vuodet eivät ole veljiä keskenään...
TODO: WIP, lisäohjeita tulossa

## Asennus / ohjeet

Tarvitaan vähintään: dotnet 6
LIsäksi: Visual studio / visual studio code


## Kehitys

```shell
git clone https://github.com/mpulukkinen/CsvPowerTemperature.git
Ajaminen komentoriviltä (esimerkiksi): dotnet run -d 15.11.2022 -cl -s -m

```

And state what happens step-by-step.

### Kääntäminen

```shell
dotnet build
```

## Ominaisuudet

Tällä hetkellä toimivat ominaisuudet
* Datan lataus csv-tiedostosta
* Puuttuvien keskilämpötilojen haku paikkauntakohtaisesti (Ilmatieteenlaitos) (TODO: tähän tarkemmat ohjeet)
* Datan ryhmittely lämpötilan mukaan
* Kahden ajanjakson tai tiedoston vertailu
* Tulosten näyttäminen konsolissa

## Kontribuoiminen

Pull requesteja otetaan vastaan...
Jos kirjoitat bugeja, loki pakollinen, muuten lentää bugirapsa roskiin ja nopeasti

## Linkit

- Github: https://github.com/mpulukkinen/CsvPowerTemperature
- Riippuvuudet:
  - https://github.com/stevehansen/csv/
  - https://github.com/serilog/serilog


## Lisenssi

MIT
