import { useState } from 'react';
import PropTypes from 'prop-types';
import { toast } from 'react-hot-toast';
import { PaymentServices } from '../../api/paymentService.js';
import { formatCurrency } from '../../utils/formatters.js';
import { CreditCard } from 'lucide-react';

export const TopUpFlow = ({ paymentMethods, onSuccess, onCancel }) => {
    const [selectedMethodId, setSelectedMethodId] = useState(paymentMethods.find(m => m.isDefault)?.id || null);
    const [amount, setAmount] = useState('');
    const [step, setStep] = useState('form');

    const handleTopUp = async (e) => {
        e.preventDefault();
        if (!selectedMethodId) {
            toast.error('Por favor, selecciona un método de pago.');
            return;
        }
        const numericAmount = parseFloat(amount);
        if (isNaN(numericAmount) || numericAmount < 1) {
            toast.error('Por favor, ingresa un monto válido (mínimo 1).');
            return;
        }

        setStep('processing');
        const topUpData = {
            paymentMethodId: selectedMethodId,
            amount: numericAmount,
        };

        PaymentServices.reloadWallet(topUpData)
            .then(response => {
                console.log("Transacción de recarga exitosa:", response.data);
                toast.success('¡Recarga realizada con éxito!');
                onSuccess();
                setStep('success');
            })
            .catch(error => {
                toast.error(error.response?.data?.message || 'La recarga no pudo ser procesada.');
                setStep('form');
            });
    };

    if (step === 'success') {
        return (
            <div className="text-center space-y-4">
                <h2 className="text-2xl font-bold text-green-600">¡Recarga Exitosa!</h2>
                <p>El monto ha sido añadido a tu saldo. Verás el saldo actualizado en tu Dashboard.</p>
                <button onClick={onCancel} className="w-full mt-4 py-2 text-white bg-indigo-600 rounded-md">Cerrar</button>
            </div>
        );
    }
    
    if (step === 'processing') {
        return (
             <div className="text-center space-y-4 py-8">
                <div className="animate-spin h-10 w-10 border-4 border-indigo-500 border-t-transparent rounded-full mx-auto"></div>
                <p className="text-lg">Procesando recarga...</p>
            </div>
        );
    }
    
    return (
        <form onSubmit={handleTopUp}>
            <h2 className="text-xl font-bold text-center mb-6">Recargar Saldo</h2>
            <div className="space-y-4">
                <div>
                    <label className="text-sm font-medium">Selecciona el método de pago</label>
                    <div className="mt-2 space-y-2">
                        {paymentMethods.map(method => (
                            <label key={method.id} htmlFor={`method-${method.id}`} className="flex items-center p-3 border rounded-md cursor-pointer has-[:checked]:bg-indigo-50 has-[:checked]:border-indigo-400">
                                <input type="radio" id={`method-${method.id}`} name="paymentMethod" value={method.id}
                                    checked={selectedMethodId === method.id}
                                    onChange={(e) => setSelectedMethodId(Number(e.target.value))}
                                    className="h-4 w-4 text-indigo-600 border-gray-300"
                                />
                                <CreditCard className="h-5 w-5 text-gray-500 mx-3" />
                                <span className="flex-1 font-mono text-sm">{paymentMethodTypeMap[method.type] || 'Método'} •••• {method.maskedIdentifier || method.last4}</span>
                                {method.isDefault && <span className="text-xs text-green-600 font-bold">Default</span>}
                            </label>
                        ))}
                    </div>
                </div>
                 <div>
                    <label htmlFor="amount" className="text-sm font-medium">Monto a Recargar (Bs.)</label>
                    <input id="amount" type="number" step="0.01" min="1" value={amount} onChange={(e) => setAmount(e.target.value)} required
                        className="w-full px-3 py-2 mt-1 border rounded-md" 
                    />
                </div>
            </div>
            <div className="flex justify-end space-x-3 pt-6">
                 <button type="button" onClick={onCancel} className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">
                    Cancelar
                </button>
                <button type="submit" disabled={!selectedMethodId || !amount} className="px-4 py-2 text-white bg-indigo-600 rounded-md hover:bg-indigo-700 disabled:bg-indigo-400">
                    Recargar {amount ? formatCurrency(parseFloat(amount)) : ''}
                </button>
            </div>
        </form>
    );
};

const paymentMethodTypeMap = {
    0: 'Tarjeta de Crédito',
    1: 'Tarjeta de Débito',
    2: 'Cuenta Bancaria',
};

TopUpFlow.propTypes = {
    paymentMethods: PropTypes.array.isRequired,
    onSuccess: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
};