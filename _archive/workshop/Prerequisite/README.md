# Prerequisites & Setup

[This page in Notion](https://timmarwick.notion.site/Prerequisites-Setup-649736e6137641c98a8491f4dfcd6ef8)

This workshop is focused on gaining hands on experience working with IaC, Azure DevOps, and Azure Cloud Resources. As such, participants should come prepared with personal Azure DevOps and Azure Portal accounts ready to go. 

There are 4 prerequisite steps, to be completed prior to the workshop. 

1. Sign up to Azure Portal
2. Create an Organisation in Azure DevOps
3. Request free parallelism grant for your DevOps Organisation
    - this can take 2 - 3 business days, so get onto it
    - the instant workaround involves adding a credit card
4. Review installed tools checklist

# Prerequisite Steps

## 1. Sign up to Azure Portal

- Go to [https://azure.microsoft.com/en-au/free/](https://azure.microsoft.com/en-au/free/) -> Start Free
    - **IMPORTANT!** Azure’s free tier offers USD200 credit to use in your first 30 days. This being the case, it’d be wise to wait at least until the start of April to do this step, so you are still within the 30 day period on the day of the workshop (21/04).
    
- Optional: Consider signing up via GitHub SSO. This is what we will demo on the day, and is very convenient.
- You may need to supply your credit card as part of the signup process
    - Don't worry, you'll not incur any charges. See the FAQ for more information

## 2. Create an Organisation in Azure DevOps

- Log into DevOps using the same account from [https://azure.microsoft.com/en-us/products/devops/?nav=min](https://azure.microsoft.com/en-us/products/devops/?nav=min)
- If logging into [dev.azure.com](http://dev.azure.com/) redirects you to Azure portal, use [https://aex.dev.azure.com/](https://aex.dev.azure.com/)

## 3. Request free parallelism grant for your DevOps Organisation

To request a free parallelism grant, fill out the following form [https://aka.ms/azpipelines-parallelism-request](https://aka.ms/azpipelines-parallelism-request)

- It may take 2 - 3 business days.
- You can see your parallel job details at the DevOps organisation level. Go to Organisation Settings -> Parallel jobs
- You may also set it up instantaneously with your credit card / subscription from Azure on the same page.
- For more info on parallel jobs, see [https://learn.microsoft.com/en-us/azure/devops/pipelines/licensing/concurrent-jobs?view=azure-devops&tabs=ms-hosted#microsoft-hosted-cicd](https://learn.microsoft.com/en-us/azure/devops/pipelines/licensing/concurrent-jobs?view=azure-devops&tabs=ms-hosted#microsoft-hosted-cicd)

## 4. Review installed tools checklist

We will be working with a basic web application (.NET & React), authoring infrastructure as code, and interacting with Azure. Please make sure you have the following tools (or equivalent) installed on your computer:

- Azure CLI - [https://learn.microsoft.com/en-us/cli/azure/install-azure-cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- Visual Studio / Rider
- VS Code
    - Recommended extension: Bicep - [https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep)
- dotnet
- node, npm

# FAQ

---

> Question:
> 

I have a Visual Studio Professional subscription associated with my Telstra Purple account, and it comes with monthly credits, why can't I use that account?

> Answer:
> 

Good question. Indeed, your Purple Visual Studio Professional subscription comes with Azure Portal access and a complimentary monthly credit allowance (about $70), which you can use to spin up resources to support your consulting work and your PD projects.

The issue comes when creating the Azure DevOps Organisation: "Azure DevOps organization creation is restricted in your Azure Active Directory"

You can try this for yourself by going here: [https://aex.dev.azure.com/](https://aex.dev.azure.com/), and clicking "Create new organization". Upon attempting to submit the new organisation form, you'll see the message "Azure DevOps organization creation is restricted in your Azure Active Directory."

In summary, for this workshop we recommend creating a personal Azure Portal and DevOps accounts, as it affords us full ownership and control of our accounts.

---

> Question:
> 

Is this going to cost me money? Why do I need to put in my credit card?

> Answer:
> 

No, if you signup to the free tier ([https://azure.microsoft.com/en-au/free/](https://azure.microsoft.com/en-au/free/) -> Start Free), which includes $200 credit, this workshop will not cost you money.

Azure operates on pay-as-you-go pricing, where you only pay for the resources you use. Upon initial signup you have no resources, so there are no fees.

If you opt to bypass Prerequisite Step 3, and instead unlock parallelism in your DevOps pipeline by entering a credit card, you'll be operating on a pay-as-you-go model for your build pipeline. You will not incur any fees until you start running a build pipeline.

Again, when you signup to the Azure free tier you'll get $200 free credit. This will be more than enough for a 1 day workshop, so it's safe to say this workshop will not cost you money.

At the end of the workshop we will allow some time for you to tear down the azure resources you've created. 

You might also choose to keep your resources running beyond the workshop, for continued PD, and in this case you will eventually start seeing charges.

---

> Question:
> 

I encountered other issues, not covered in this documentation. Or, I have other suggestions to improve this documentation. Can I contribute to the this FAQ?

> Answer:
> 

Yes please do. See [Contact & Feedback](https://www.notion.so/Contact-Feedback-16134d889ca647f6a277098e1f470e4f) 

### Next: [Contact & Feedback](https://www.notion.so/Contact-Feedback-16134d889ca647f6a277098e1f470e4f)