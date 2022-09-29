using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON; // SimpleJSON.cs
using UnityEngine.UI;
using TMPro;

public class ProductAPIController : MonoBehaviour
{
    public int selectedProductIndex;
    public RawImage productRawImage;
    public TextMeshProUGUI productIndexText, productNameText, productDescriptionText, promoPriceText, normalPriceText;
    public TextMeshProUGUI[] productSpecsTextArray; // Brand, Colour, SKU and Warranty

    private readonly string baseProductURL = "https://mystery24.github.io/metaverse-product-api/"; // Base URL of the API
    
    private void Start()
    {
        productRawImage.texture = Texture2D.blackTexture; // To hide the raw image

        // Set the loading message
        productIndexText.text = "indexTxtNull";
        productNameText.text = "Loading...";
        promoPriceText.text = "Calculating the best price...";
        normalPriceText.text = "priceTxtNull";
        productDescriptionText.text = "Please wait... Generating product description...";

         foreach (TextMeshProUGUI productSpecText in productSpecsTextArray)
        {
            productSpecText.text = "specTxtNull";
        }

        StartCoroutine(GetProductAtIndex(selectedProductIndex));
    }

    // For testing purpose
    public void OnButtonPreviousItem()
    {
        if(selectedProductIndex > 0)
        {
            selectedProductIndex -= 1;
            Debug.Log("Index: " + selectedProductIndex);
        } else selectedProductIndex = 1;
        
        productRawImage.texture = Texture2D.blackTexture; // To hide the raw image

        // Set the loading message
        productIndexText.text = "indexTxtNull";
        productNameText.text = "Loading...";
        promoPriceText.text = "Calculating the best price...";
        normalPriceText.text = "priceTxtNull";
        productDescriptionText.text = "Please wait... Generating product description...";

         foreach (TextMeshProUGUI productSpecText in productSpecsTextArray)
        {
            productSpecText.text = "specTxtNull";
        }

        StartCoroutine(GetProductAtIndex(selectedProductIndex));
    }

    // For testing purpose
    public void OnButtonNextItem()
    {
        // Current JSON file until 26.json
        if(selectedProductIndex < 27)
        {
            selectedProductIndex += 1;
            Debug.Log("Index: " + selectedProductIndex);
        } else selectedProductIndex = 26;
       
        productRawImage.texture = Texture2D.blackTexture; // To hide the raw image

        // Set the loading message
        productIndexText.text = "indexTxtNull";
        productNameText.text = "Loading...";
        promoPriceText.text = "Calculating the best price...";
        normalPriceText.text = "priceTxtNull";
        productDescriptionText.text = "Please wait... Generating product description...";

         foreach (TextMeshProUGUI productSpecText in productSpecsTextArray)
        {
            productSpecText.text = "specTxtNull";
        }

        StartCoroutine(GetProductAtIndex(selectedProductIndex));
    }

    // For testing purpose
    public void OnButtonRefreshItem()
    {
        // int randomProductIndex = Random.Range(1, 808); // Min: inclusive, Max: exclusive
        // selectedProductIndex = 1;

        productRawImage.texture = Texture2D.blackTexture; // To hide the raw image

        // Set the loading message
        productIndexText.text = "indexTxtNull";
        productNameText.text = "Loading...";
        promoPriceText.text = "Calculating the best price...";
        normalPriceText.text = "priceTxtNull";
        productDescriptionText.text = "Please wait... Generating product description...";

         foreach (TextMeshProUGUI productSpecText in productSpecsTextArray)
        {
            productSpecText.text = "specTxtNull";
        }

        StartCoroutine(GetProductAtIndex(selectedProductIndex));
    }

    IEnumerator GetProductAtIndex(int productIndex)
    {
        // Get product info
        string productURL = baseProductURL + "data/" + productIndex.ToString() + ".json";
        // Example URL: https://mystery24.github.io/metaverse-product-api/data/1.json

        // Create a UnityWebRequest to access a website and use UnityWebRequest.Get to download a page
        UnityWebRequest productInfoRequest = UnityWebRequest.Get(productURL);

        // Request and wait for the desired page
        yield return productInfoRequest.SendWebRequest();

        // Check for error. If something goes wrong, exit the coroutine. Otherwise, continue to parse the JSON.
        switch (productInfoRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                Debug.LogError(productURL + ": URL: " + productInfoRequest.error);
                break;
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(productURL + ": Error: " + productInfoRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(productURL + ": HTTP Error: " + productInfoRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log(productURL + ":\nSuccess: " + productInfoRequest.downloadHandler.text);
                break;
        }

        // Parse the JSON file
        JSONNode productInfo = JSON.Parse(productInfoRequest.downloadHandler.text);

        string productSpriteURL = productInfo["sprites"]["default"];
        string productFileIndex = productInfo["index"]; 
        string productName = productInfo["name"];
        string productPrice = productInfo["promo_price"];
        string productSRPrice = productInfo["srp"];
        string productDescription = productInfo["description"];

        JSONNode productSpecs = productInfo["specs"];
        string[] productSpecColumns = new string[productSpecs.Count];

         for (int i = 0, j = productSpecs.Count - 1; i < productSpecs.Count; i++, j--)
        {
            productSpecColumns[j] = productSpecs[i]["spec"]["value"];
        }

        // Get product sprite which is stored in a JSON as URL
        UnityWebRequest productSpriteRequest = UnityWebRequestTexture.GetTexture(productSpriteURL);

        yield return productSpriteRequest.SendWebRequest();    

        // Check for error. If something goes wrong, exit the coroutine. Otherwise, set the UI objects.
        // if (productSpriteRequest.isNetworkError || productSpriteRequest.isHttpError)
        if (productSpriteRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(productSpriteURL + ": Sprite download error: " + productSpriteRequest.error);
            yield break;
        }

        // Set UI objects
        productRawImage.texture = DownloadHandlerTexture.GetContent(productSpriteRequest);
        productRawImage.texture.filterMode = FilterMode.Point; // Apply no filtering to the image

        productIndexText.text = "#" + productFileIndex; // Example: #0001
        productNameText.text = productName; // All caps
        promoPriceText.text = "RM " + productPrice; // Example: RM 59.00
        normalPriceText.text = "SRP: RM " + productSRPrice;
        productDescriptionText.text = CapitalizeFirstLetter(productDescription);  

        for (int i = 0; i < productSpecColumns.Length; i++)
        {
            productSpecsTextArray[i].text = CapitalizeFirstLetter(productSpecColumns[i]);
        }

    }

    private string CapitalizeFirstLetter(string str)
    {
        return char.ToUpper(str[0]) + str.Substring(1);
    }

}
