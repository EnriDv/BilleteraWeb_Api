import apiClient from "./apiClient.js";

export const UserServices = {
    changePin : (input) => {
        const pinData = {
            currentPin: input.currentPin,
            newPin: input.newPin,
        };
        return apiClient.put('/user/change-pin', pinData);
    },
    getTransactionHistory : () => {
        return apiClient.get('/user/history');
    },
    sendMoney : (input) => {
        const requestData = {
            recipientPhoneNumber: input.recipientPhoneNumber,
            amount: input.amount,
            description: input.description
        };
        return apiClient.post('/user/send-money', requestData);
    },
    getBalance : () => {
        return apiClient.get('/user/balance');
    },
    acceptTransaction: (transactionId) => {
        return apiClient.put('/user/accept-transaction', { transactionId: Number(transactionId) });
    }
}