![S1C  Logo White BG - cropped_160x69](https://user-images.githubusercontent.com/55806446/179864709-875a50b6-9941-4354-ba81-597be7d48a16.png)
# JiraToDocsign
The S1C DocuSign integration automates the delivery of our project sign-off documents to the customer. Once the document is signed, the task in Jira automatically moves Ready for Production. This action then triggers automatic invoicing as a part of a different integration.
The following Jira fields must be populated to get picked up for automation:
 
 


•	Project Stage Approver – Name of the individual to sign the document

•	Approval Email – Email where the document should be sent

•	DocuSign – Indicates that this task should be automatet

•	Signoff form – The document that will be sent

     o	The document for each stage (Rapid Prototyping 1 to Project Launch) is stored in the Stellar One Cloud. New or changed documents must be loaded into the integration database to take effect.

     o	Each document has a series of tags that informs DocuSign where various tags are in the document. The tag definitions are stored in the integration database as well.

     o	Carbon Copy settings are maintained in the integration database

•	Fix versions – The name of the project. This value will be included in the email and name of the document. Fix version is found in the main details section if the task and not in the Billing section along with the other fields
When these fields are populated, the integration will send the appropriate sign-off form once the task is set to UA-Testing.
Once the document is signed, the integration will move the task to Ready for Production.


